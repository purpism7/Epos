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

            // Flip();
            SetAnimation(_data.AnimationKey, true);
            
            if (_iActor?.NavMeshAgent != null &&
                !_data.IsJumpMove)
            {
                _iActor.NavMeshAgent.speed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
                _iActor?.NavMeshAgent?.SetDestination(_data.TargetPos);
            }
        }

        // private void Flip()
        // {
        //     var iActorTm = _iActor?.NavMeshAgent?.transform;
        //     if (!iActorTm)
        //         return;
        //
        //     // var rigidbody = _iActor.Rigidbody2D;
        //     // if (rigidbody == null)
        //     //     return;
        //     
        //     var direction = _data.TargetPos.x - iActorTm.position.x;
        //     iActorTm.localScale = new Vector3(direction < 0 ? -1f : 1f, 1f, 1f);
        // }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            if (_iActor?.IStat == null)
                return;
            
            if (_data.IsJumpMove)
            {
                Vector2 targetPos = _data.TargetPos;
                var moveSpeed = _iActor.IStat.Get(Stat.EType.MoveSpeed);

                iActorTm.position = Vector3.Lerp(iActorTm.position, targetPos, Time.deltaTime * moveSpeed);
                // iActorTm.position = Vector3.MoveTowards();
            }
            
            Vector2 direction = _data.TargetPos - iActorTm.position;
            if (direction.x > 0)
                iActorTm.localScale = new Vector3(1, 1, 1);
            else if (direction.x < 0)
                iActorTm.localScale = new Vector3(-1, 1, 1);
            
            var distance = Vector2.Distance(iActorTm.position, _data.TargetPos);
            if (distance < 1f)
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
