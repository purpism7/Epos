using UnityEngine.InputSystem;

using Battles.Simulation;
using Managers;

namespace Battles
{
    /// <summary>
    /// 서바이벌 장면의 판 흐름: 시작하면 바로 싸운다 → 한쪽이 모두 전투 불능이면 결과를 띄운다 → R 키로 같은 배치에서 다시.
    /// 승패를 가르는 규칙은 시뮬 쪽(BattleOutcomeJudge)에 있고, 여기는 결과 화면과 다시 하기 입력만 잇는다.
    /// </summary>
    public sealed class SurvivalMode : BattleMode
    {
        private IBattleManager _battleManager = null;
        private BattleResultView _resultView = null;

        public void Initialize(IBattleManager battleManager, BattleResultView resultView)
        {
            _battleManager = battleManager;
            _resultView = resultView;
        }

        public override void Activate()
        {
            if (_battleManager == null)
                return;

            _battleManager.OnBattleEnded -= OnBattleEnded;
            _battleManager.OnBattleEnded += OnBattleEnded;

            // 켜기 전에 이미 끝난 판이면 결과를 바로 띄운다.
            if (_battleManager.Outcome != BattleOutcome.Ongoing)
                _resultView?.Show(_battleManager.Outcome);

            base.Activate();
        }

        public override void Deactivate()
        {
            if (_battleManager != null)
                _battleManager.OnBattleEnded -= OnBattleEnded;

            base.Deactivate();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            // 다시 하기는 판이 끝난 뒤에만 받는다.
            if (!IsActive || _battleManager == null || _battleManager.Outcome == BattleOutcome.Ongoing)
                return;

            if (!WasRestartPressed())
                return;

            _resultView?.Hide();
            _battleManager.Restart();
        }

        private void OnBattleEnded(BattleOutcome outcome)
        {
            _resultView?.Show(outcome);
        }

        private static bool WasRestartPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.rKey.wasPressedThisFrame;
        }
    }
}
