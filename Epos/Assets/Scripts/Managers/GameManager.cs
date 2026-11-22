using System;
using UnityEngine;

using Battles;
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
        
        [Tooltip("전투 규칙 ⚑(임시값): 1초 틱 수 · 표적을 다시 고르는 주기. 플레이를 시작할 때 한 번 읽는다.")]
        [SerializeField] private BattleRulesSettings battleRules = new BattleRulesSettings();

        [Tooltip("타격 · 전투 불능 이벤트를 콘솔에 찍는다(HP UI 가 생기기 전 확인용). 이벤트마다 문자열을 만들므로 성능을 잴 때는 끈다.")]
        [SerializeField] private bool logBattleEvents = true;

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
            
            _battleManager?.Initialize(_characterManager, (battleRules ?? new BattleRulesSettings()).ToBattleRules(), logBattleEvents);

            Activate();
        }

        public override void Activate()
        {
            _inputManager?.Activate();
            _cameraManager?.Activate();
            _characterManager?.Activate();
            _battleManager?.Activate();

            base.Activate();
        }

        public override void Deactivate()
        {
            _inputManager?.Deactivate();
            _cameraManager?.Deactivate();
            _characterManager?.Deactivate();
            _battleManager?.Deactivate();

            base.Deactivate();
        }

        // Update is called once per frame
        void Update()
        {
            _characterManager?.ChainUpdate();
            _inputManager?.ChainUpdate();
            // 입력(이동 의도)을 읽은 뒤 고정 틱을 돌리고 결과를 캐릭터 그림에 옮긴다.
            _battleManager?.ChainUpdate();
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
