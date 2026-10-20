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
            
            if (PlayAnimation("Run", true))
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

            _actor?.ActionController?.Flip(axis.x);
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
            PlayAnimation("Idle", true);

            Debug.Log("Move Completed");
        }
    }
}