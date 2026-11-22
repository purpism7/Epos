using UnityEngine;
using System;
using System.Collections.Generic;

using Characters;
using Commons;

namespace Managers
{
    public class CharacterManager : Entity
    {
        public event Action<PlayerCharacter> OnMainPlayerChanged = null;

        /// <summary>씬의 모든 캐릭터(플레이어 · 몬스터). 실행마다 같은 순서(CompareForUnitOrder)이고, 전투 유닛 번호가 이 순서로 매겨진다.</summary>
        public IReadOnlyList<Character> AllCharacters => _characters;

        private readonly List<Character> _characters = new List<Character>();
        private PlayerCharacter _mainPlayer = null;
        
        public void Initialize()
        {
            var players = GameObject.FindObjectsByType<PlayerCharacter>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            Debug.Log($"찾은 PlayerCharacter 개수: {players.Length}");

            foreach (var player in players)
            {
                Debug.Log($"찾은 캐릭터: {player.gameObject.name}");
            }

            _mainPlayer = GameObject.FindAnyObjectByType<PlayerCharacter>(
                FindObjectsInactive.Include
            );
            
            // 메인 플레이어만이 아니라 씬의 모든 캐릭터(몬스터 포함)를 찾아 정해진 순서로 초기화한다. 메인 플레이어도 이 목록에 들어 있다.
            CollectCharacters();
        }

        public override void Activate()
        {
            ChangedMainPlayer(_mainPlayer);
            
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
            
            for (int i = 0; i < _characters.Count; i++)
            {
                var character = _characters[i];
                if (character != null)
                    character.ChainUpdate();
            }
        }
        
        public void ChangedMainPlayer(PlayerCharacter player)
        {
            if (player == null)
                return;
            
            _mainPlayer = player;
            
            OnMainPlayerChanged?.Invoke(player);
        }

        // 씬의 모든 캐릭터(플레이어 · 몬스터)를 찾아 정해진 순서로 줄 세우고 초기화한다.
        private void CollectCharacters()
        {
            _characters.Clear();
            _characters.AddRange(GameObject.FindObjectsByType<Character>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ));

            // 찾는 순서는 정해져 있지 않다. 전투 유닛 번호(= 이 순서)가 실행마다 같도록 정렬한다.
            _characters.Sort(CompareForUnitOrder);

            for (int i = 0; i < _characters.Count; i++)
                _characters[i].Initialize();

            Debug.Log($"찾은 Character 개수: {_characters.Count}");
        }

        // 플레이어 먼저 → 이름(서수) → 위치 x → y. 같은 씬이면 늘 같은 순서가 된다(조작하는 기사가 0번).
        private static int CompareForUnitOrder(Character a, Character b)
        {
            int byKind = GetKindOrder(a).CompareTo(GetKindOrder(b));
            if (byKind != 0)
                return byKind;

            int byName = string.CompareOrdinal(a.name, b.name);
            if (byName != 0)
                return byName;

            Vector3 positionA = a.transform.position;
            Vector3 positionB = b.transform.position;

            int byX = positionA.x.CompareTo(positionB.x);
            return byX != 0 ? byX : positionA.y.CompareTo(positionB.y);
        }

        private static int GetKindOrder(Character character)
        {
            return character is PlayerCharacter ? 0 : 1;
        }
    }
}

