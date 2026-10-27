using UnityEngine;
using System;

using Characters;
using Commons;

namespace Managers
{
    public class CharacterManager : Entity
    {
        public event Action<PlayerCharacter> OnMainPlayerChanged = null;

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
            
            _mainPlayer?.Initialize();
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
            
            _mainPlayer?.ChainUpdate();
        }
        
        public void ChangedMainPlayer(PlayerCharacter player)
        {
            if (player == null)
                return;
            
            _mainPlayer = player;
            
            OnMainPlayerChanged?.Invoke(player);
        }
    }
}

