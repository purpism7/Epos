using UnityEngine;

namespace Characters.Actions
{
    public class MoveParam : ActionParam
    {
        public Vector2 MoveAxis = Vector2.zero;
    }

    public class Move : Action<MoveParam>
    {
        private const float ArriveDistance = 0.05f;

        private bool _isMoving = false;

        public override void Execute()
        {
            if (_actor == null || _actionParam == null)
                return;
            
            // 이동 방향을 먼저 본 뒤 한 번만 재생한다 → 첫 프레임부터 맞는 앞/뒤 달리기가 나온다.
            if (PlayAnimation(AnimationKey.Run, true, _actionParam.MoveAxis))
                _isMoving = true;
        }

        public override void ChainUpdate()
        {
            // 위치 · 바라보는 쪽 · 그리기 순서는 전투 시뮬 결과로 BattleUnitPresenter 가 프레임마다 맞춘다.
            // 이동 액션은 달리기 그림만 맡는다(Execute). 속도는 데이터(BattleStatsSettings 의 moveSpeed)로 옮겼다.
            return;
        }

        protected override void EndAction()
        {
            base.EndAction();

            _isMoving = false;
        }

        private void CompleteMove()
        {
            if (!_isMoving)
                return;

            _isMoving = false;

            // TODO: Idle 애니메이션 재생 또는 액션 완료 이벤트 호출
            PlayAnimation(AnimationKey.Idle, true);

            Debug.Log("Move Completed");
        }
    }
}