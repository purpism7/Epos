using UnityEngine;
using System;

using Actor;
using Characters;

namespace Managers
{
    public class CharacterManager : IManager
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
        }

        public void Activate()
        {
            ChangedMainPlayer(_mainPlayer);
        }

        public void Deactivate()
        {
            
        }

        public void ChainUpdate()
        {
            _mainPlayer?.ChainUpdate();
        }

        public void ChainLateUpdate()
        {
            
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

