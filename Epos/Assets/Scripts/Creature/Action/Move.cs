using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Creature.Action
{
    public class Move : Act<Move.Data>
    {
        public class Data : BaseData
        {
            public float MoveSpeed = 1f;
            public Transform TargetTm { get; private set; } = null;
            public Vector3? TargetPos = null;
            public Vector3? OffsetPosition { get; private set; } = null;
            public System.Action FinishAction = null;
            public bool IsJumpMove = false;
            public bool UseNavMesh = true;

            public int DirectionAfterArriving = 1;

            public Data WithTargetTm(Transform targetTm)
            {
                TargetTm = targetTm;
                return this;
            }

            public Data WithOffsetPosition(Vector3 position)
            {
                OffsetPosition = position;
                return this;
            }
        }

        private Vector3 _prevPos = Vector3.zero;
        
        public bool IsJumpMove { get { return _data != null ? _data.IsJumpMove : false; } }

        public override void Execute()
        {
            if (_data == null)
                return;

            // Flip();
            SetAnimation(_data.AnimationKey, true);

            if (_iActor?.NavMeshAgent != null &&
                _data != null &&
                _data.UseNavMesh)
            {
                var targetPos = Vector2.zero;
                if (_data.TargetTm)
                    targetPos = _data.TargetTm.position;

                if (_data.TargetPos != null)
                    targetPos = _data.TargetPos.Value;

                _iActor.NavMeshAgent.speed = _data.MoveSpeed;
                _iActor?.NavMeshAgent?.SetDestination(targetPos);
            }

            if (_iActor?.Transform)
                _prevPos = _iActor.Transform.position;
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

            if (_data != null &&
               _data.UseNavMesh)
                return;

            if (_iActor?.IStat == null)
                return;

            Vector3 targetPos = Vector3.zero;
            if (_data.TargetTm)
                targetPos = _data.TargetTm.position;

            if (_data.TargetPos != null)
                targetPos = _data.TargetPos.Value;

            var direction = targetPos - iActorTm.position;
            Vector3 offsetPosition = Vector3.zero;
            float offsetDistance = 0;

            if (_data.OffsetPosition != null)
            {
                offsetPosition = _data.OffsetPosition.Value;
                offsetDistance = offsetPosition.x;

                targetPos.x = direction.x <= 0 ? targetPos.x + offsetPosition.x : targetPos.x - offsetPosition.x;
                targetPos.y += offsetPosition.y;
            }

            //Debug.Log(offsetPosition);

            var moveSpeed = _data.MoveSpeed;
            iActorTm.position = Vector2.MoveTowards(iActorTm.position, targetPos, moveSpeed * Time.deltaTime);

            direction = _prevPos - iActorTm.position;
            if (direction.x > 0)
                iActorTm.localScale = new Vector3(-1, 1, 1);
            else if (direction.x < 0)
                iActorTm.localScale = Vector3.one;

            _prevPos = iActorTm.position;
            
            var distance = Vector2.Distance(iActorTm.position, targetPos);
            if (distance < offsetDistance)
            {
                // 도착 후, 현재 바라보는 방향과 반대로 바라보기.
                var localScale = iActorTm.localScale;
                localScale.x = _data.DirectionAfterArriving;
                    
                iActorTm.localScale = localScale;
                
                _data.FinishAction?.Invoke();
                _endAction?.Invoke();
            }
        }
    }
}
