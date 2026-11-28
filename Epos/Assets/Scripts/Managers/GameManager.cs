using System;
using TMPro;
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
        private readonly SurvivalMode _survivalMode = new SurvivalMode();

        [Tooltip("전투 규칙 ⚑(임시값): 1초 틱 수 · 표적을 다시 고르는 주기. 플레이를 시작할 때 한 번 읽는다.")]
        [SerializeField] private BattleRulesSettings battleRules = new BattleRulesSettings();

        [Tooltip("타격 · 전투 불능 이벤트를 콘솔에 찍는다(HP UI 가 생기기 전 확인용). 이벤트마다 문자열을 만들므로 성능을 잴 때는 끈다.")]
        [SerializeField] private bool logBattleEvents = true;

        [Tooltip("판이 끝났을 때 띄우는 결과(승리 · 패배 · 무승부 · R 다시 하기)의 글꼴. 한글 글리프가 있어야 한다. 비우면 TMP 기본 글꼴(한글이 네모로 보일 수 있음).")]
        [SerializeField] private TMP_FontAsset battleResultFont = null;

        private GameState _gameState = null;

        private void Awake()
        {
            Debug.Log("GameManager Awake");
        }

        private void Start()
        {
            // 임시
            // GameInitializer 가 장면을 읽기 전에 만들기 때문에 Awake 에서는 장면 오브젝트가 아직 깨어나지 않아 비활성으로 보인다
            // (꺼진 캐릭터로 여겨 전투에 넣지 못하고, 자식 컴포넌트 찾기도 실패할 수 있다). 장면이 모두 깨어난 뒤인 Start 에서 초기화한다.
            Initialize();
        }

        private void Initialize()
        {
            _characterManager?.Initialize();
            _inputManager?.Initialize(_characterManager);
            _cameraManager?.Initialize(_characterManager);
            
            _battleManager?.Initialize(_characterManager, (battleRules ?? new BattleRulesSettings()).ToBattleRules(), logBattleEvents);
            _survivalMode?.Initialize(_battleManager, new BattleResultView(battleResultFont));

            Activate();
        }

        public override void Activate()
        {
            _inputManager?.Activate();
            _cameraManager?.Activate();
            _characterManager?.Activate();
            _battleManager?.Activate();
            _survivalMode?.Activate();

            base.Activate();
        }

        public override void Deactivate()
        {
            _inputManager?.Deactivate();
            _cameraManager?.Deactivate();
            _characterManager?.Deactivate();
            _battleManager?.Deactivate();
            _survivalMode?.Deactivate();

            base.Deactivate();
        }

        // Update is called once per frame
        void Update()
        {
            _characterManager?.ChainUpdate();
            _inputManager?.ChainUpdate();
            // 입력(이동 의도)을 읽은 뒤 고정 틱을 돌리고 결과를 캐릭터 그림에 옮긴다.
            _battleManager?.ChainUpdate();
            // 판 흐름(끝났으면 결과 · R 다시 하기). 다시 하면 다음 프레임부터 새 판의 틱이 돈다.
            _survivalMode?.ChainUpdate();
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
