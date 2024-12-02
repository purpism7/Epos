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
            public bool IsJumpMove = false;
        }

        public bool IsJumpMove { get { return _data != null ? _data.IsJumpMove : false; } }

        public override void Execute()
        {
            if (_data == null)
                return;

            if (_iActor?.NavMeshAgent == null)
                return;
            
            Flip();
            
            SetAnimation(_data.AnimationKey, true);
            
            _iActor.NavMeshAgent.speed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
            _iActor?.NavMeshAgent?.SetDestination(_data.TargetPos);
        }

        private void Flip()
        {
            var iActorTm = _iActor?.NavMeshAgent?.transform;
            if (!iActorTm)
                return;

            // var rigidbody = _iActor.Rigidbody2D;
            // if (rigidbody == null)
            //     return;
            
            var direction = _data.TargetPos.x - iActorTm.position.x;
            iActorTm.localScale = new Vector3(direction < 0 ? -1f : 1f, 1f, 1f);
        }

        public override void ChainFixedUpdate()
        {
            base.ChainFixedUpdate();

            var iActorTm = _iActor?.NavMeshAgent?.transform;
            if (!iActorTm)
                return;

            // var rigidbody = _iActor.Rigidbody2D;
            // if (rigidbody == null)
            //     return;
            
            if (_iActor?.IStat == null)
                return;

            // Vector2 targetPos = _data.TargetPos;
            // var moveSpeed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
            // var direction = (targetPos - rigidbody.position).normalized;
            //
            // Vector2 resPos = rigidbody.position + direction * moveSpeed * Time.fixedDeltaTime;
            // rigidbody.MovePosition(resPos);
            
            // iActorTm.position = Vector3.MoveTowards(iActorTm.position, targetPos, Time.deltaTime * moveSpeed);
            var distance = Vector2.Distance(iActorTm.position, _data.TargetPos);
            
            // Debug.Log(distance);
            if (distance < 0.1f)
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
