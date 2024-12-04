using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            
            // Flip();
            
            SetAnimation(_data.AnimationKey, true);
            
            if (!_data.IsJumpMove)
            {
                _iActor.NavMeshAgent.speed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
                _iActor?.NavMeshAgent?.SetDestination(_data.TargetPos);
            }
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

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            // var rigidbody = _iActor.Rigidbody2D;
            // if (rigidbody == null)
            //     return;
            
            if (_iActor?.IStat == null)
                return;

            // if (!_data.IsJumpMove)
            //     return;

            
            if (_data.IsJumpMove)
            {
                Vector2 targetPos = _data.TargetPos;
                var moveSpeed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
                
                iActorTm.position = Vector3.MoveTowards(iActorTm.position, targetPos, Time.deltaTime * moveSpeed);
            }
            
            Vector2 direction = _data.TargetPos - iActorTm.position;
            // 방향 벡터의 각도 계산 (2D에서 회전 각도는 Z축을 기준으로)
            // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // // 오브젝트의 회전 설정 (Z축 회전)
            // iActorTm.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            
            if (direction.x > 0)
                iActorTm.localScale = new Vector3(1, 1, 1);
            else if (direction.x < 0)
                iActorTm.localScale = new Vector3(-1, 1, 1);
            
            var distance = Vector2.Distance(iActorTm.position, _data.TargetPos);
            
            // var direction = (targetPos - rigidbody.position).normalized;
            //
            // Vector2 resPos = rigidbody.position + direction * moveSpeed * Time.fixedDeltaTime;
            // rigidbody.MovePosition(resPos);

            if (_iActor?.NavMeshAgent != null)
            {
                // distance = _iActor.NavMeshAgent.remainingDistance;
            }
            
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
