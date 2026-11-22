using UnityEngine;

namespace Characters.Actions
{
    public class MoveParam : ActionParam
    {
        public Vector2 MoveAxis = Vector2.zero;
    }

    public class Move : Action<MoveParam>
    {
        private const float MoveSpeed = 5f;
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
            if (_actor == null || _actionParam == null)
                return;

            Vector2 axis = _actionParam.MoveAxis;
            if (axis == Vector2.zero)
            {
                EndAction();
                return;
            }

            var transform = _actor.Transform;
            if (transform == null)
            {
                EndAction();
                return;
            }

            Vector3 movement = new Vector3(axis.x, axis.y, 0f);
            
            // var direction = movement - transform.position;

            // 좌우 반전 + 앞뒤 갱신. 앞뒤가 바뀌면 Execute 가 다시 불리지 않아도 달리기를 그쪽 그림으로 바꿔 튼다.
            _actor?.CharacterAnimator?.Face(axis);
            _actor?.SetSortingOrder(transform.position.y);

            transform.position += movement * MoveSpeed * Time.deltaTime;
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