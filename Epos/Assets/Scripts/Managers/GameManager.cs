using System;
using UnityEngine;

using GameStates;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private readonly CharacterManager _characterManager = new CharacterManager();
        private readonly InputManager _inputManager = new InputManager();
        
        private GameState _gameState = null;

        private void Awake()
        {
            Debug.Log("GameManager Awake");
            
            // 임시 
            Initialize();
        }

        private void Initialize()
        {
            _inputManager?.Initialize(_characterManager);
            _characterManager?.Initialize();

            Activate();
        }

        private void Activate()
        {
            _inputManager?.Activate();
            _characterManager?.Activate();
        }

        private void Deactivate()
        {
            _inputManager?.Deactivate();
            _characterManager?.Deactivate();
        }

        // Update is called once per frame
        void Update()
        {
            _characterManager?.ChainUpdate();
            _inputManager?.ChainUpdate();
        }

        public void SetGameState(GameState gameState)
        {
            
        }
    }
}
