using System;
using UnityEngine;

using GameStates;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private readonly InputManager _inputManager = new InputManager();
        
        private GameState _gameState = null;

        private void Awake()
        {
            Debug.Log("GameManager Awake");
            
            // 임시 
            _inputManager?.Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            _inputManager?.ChainUpdate();
        }

        public void SetGameState(GameState gameState)
        {
            
        }
    }
}
