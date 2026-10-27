using System;
using UnityEngine;

using Commons;
using GameStates;

namespace Managers
{
    public class GameManager : MonoEntity
    {
        private readonly CharacterManager _characterManager = new CharacterManager();
        private readonly CameraManager _cameraManager = new CameraManager();
        private readonly InputManager _inputManager = new InputManager();

        private readonly BattleManager _battleManager = new BattleManager();
        
        private GameState _gameState = null;

        private void Awake()
        {
            Debug.Log("GameManager Awake");
            
            // 임시 
            Initialize();
        }

        private void Initialize()
        {
            _characterManager?.Initialize();
            _inputManager?.Initialize(_characterManager);
            _cameraManager?.Initialize(_characterManager);
            
            _battleManager?.Initialize();

            Activate();
        }

        public override void Activate()
        {
            _inputManager?.Activate();
            _cameraManager?.Activate();
            _characterManager?.Activate();
            
            base.Activate();
        }

        public override void Deactivate()
        {
            _inputManager?.Deactivate();
            _cameraManager?.Deactivate();
            _characterManager?.Deactivate();
            
            base.Deactivate();
        }

        // Update is called once per frame
        void Update()
        {
            _characterManager?.ChainUpdate();
            _inputManager?.ChainUpdate();
        }

        private void LateUpdate()
        {
            _cameraManager?.ChainLateUpdate();
        }

        public void SetGameState(GameState gameState)
        {
            
        }
    }
}
