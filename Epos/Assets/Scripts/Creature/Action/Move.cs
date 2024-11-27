using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Move : Act<Move.Data>
    {
        public class Data : BaseData
        {
            public Vector3 TargetPos = Vector3.zero;
            public System.Action FinishAction = null;
            public bool ReverseAfterArriving = false;
        }
        
        public override void Execute()
        {
            if (_data == null)
                return;
            
            Flip();
            
            SetAnimation(_data.AnimationKey, true);
        }

        private void Flip()
        {
            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            var rigidbody = _iActor.Rigidbody2D;
            if (rigidbody == null)
                return;
            
            var direction = _data.TargetPos.x - rigidbody.position.x;
            iActorTm.localScale = new Vector3(direction < 0 ? -1f : 1f, 1f, 1f);
        }

        public override void ChainFixedUpdate()
        {
            base.ChainFixedUpdate();

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            var rigidbody = _iActor.Rigidbody2D;
            if (rigidbody == null)
                return;
            
            if (_iActor?.IStat == null)
                return;

            Vector2 targetPos = _data.TargetPos;
            var moveSpeed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
            var direction = (targetPos - rigidbody.position).normalized;
            
            Vector2 resPos = rigidbody.position + direction * moveSpeed * Time.fixedDeltaTime;
            rigidbody.MovePosition(resPos);
            
            // iActorTm.position = Vector3.MoveTowards(iActorTm.position, targetPos, Time.deltaTime * moveSpeed);
            if (Vector2.Distance(rigidbody.position, targetPos) <= 0.1f)
            {
                // 도착 후, 현재 바라보는 방향과 반대로 바라보기.
                if (_data.ReverseAfterArriving)
                {
                    var localScale = iActorTm.localScale;
                    localScale.x *= -1f;
                    
                    iActorTm.localScale = localScale;
                }
                
                _data.FinishAction?.Invoke();
                
                _endAction?.Invoke();
            }
        }
    }
}
