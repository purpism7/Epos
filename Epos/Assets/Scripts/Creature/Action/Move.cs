using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

using Spine;

namespace Creature.Action
{
    public class Move : Act<Move.Param>
    {
        public class Param : ActParam
        {
            public float MoveSpeed = 1f;
            public Transform TargetTm { get; private set; } = null;
            public ICombatant TargetICombatant { get; private set; } = null;
            public Vector3? TargetPos = null;
            // public Vector3? OffsetPosition { get; private set; } = null;
            public bool ForwardDirection { get; private set; } = false;
            public float Distance { get; private set; } = 0;

            public System.Action FinishAction = null;
            public bool IsJumpMove = false;
            public bool UseNavMesh { get; private set; } = true;
            
            public int DirectionAfterArriving = 1;

            public Param WithTargetTm(Transform targetTm)
            {
                TargetTm = targetTm;
                return this;
            }

            public Param WithTargetICombatant(ICombatant targetICombatant)
            {
                TargetICombatant = targetICombatant;
                return this;
            }

            // public Param WithOffsetPosition(Vector3 position)
            // {
            //     OffsetPosition = position;
            //     return this;
            // }

            public Param WithUseNavMesh(bool useNavMesh)
            {
                UseNavMesh = useNavMesh;
                return this;
            }

            public Param WithForwardDirection(bool forwardDirection)
            {
                ForwardDirection = forwardDirection;
                return this;
            }

            public Param WithDistance(float distance)
            {
                Distance = distance;
                return this;
            }
        }

        private Vector3 _prevPos = Vector3.zero;
        private Vector3 _targetPos = Vector3.zero;
        
        public bool IsJumpMove { get { return _param != null ? _param.IsJumpMove : false; } }

        public override void Execute()
        {
            if (_param == null)
                return;
            
            Activate();
            SetAnimation(_param.AnimationKey, true);

            _targetPos = CalcTargetPos;

            if (_param != null &&
                _param.UseNavMesh)
            {
                var navMeshAgent = _iActor?.NavMeshAgent;
                if(navMeshAgent != null)
                {
                    EnableNavMeshAgent();

                    navMeshAgent.speed = _param.MoveSpeed;
                    navMeshAgent.SetDestination(_targetPos);
                }
            }
            else
                DisabledNavMeshAgent();

            if (_iActor?.Transform)
                _prevPos = _iActor.Transform.position;
        }

        protected override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            DisabledNavMeshAgent();
        }

        private void EnableNavMeshAgent()
        {
            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;
            }
        }

        private void DisabledNavMeshAgent()
        {
            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent != null &&
                navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
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
        //     var direction = _param.TargetPos.x - iActorTm.position.x;
        //     iActorTm.localScale = new Vector3(direction < 0 ? -1f : 1f, 1f, 1f);
        // }

        private Vector3 TargetPos
        {
            get
            {
                Vector3 targetPos = Vector3.zero;
                if(_param != null)
                {
                    if (_param.TargetTm)
                        targetPos = _param.TargetTm.position;

                    if (_param.TargetPos != null)
                        targetPos = _param.TargetPos.Value;

                    if (_param.TargetICombatant != null)
                        targetPos = _param.TargetICombatant.Transform.position;
                }

                return targetPos;
            }
        }

        private Vector3 CalcTargetPos
        {
            get
            {
                if (_iActor == null)
                    return Vector3.zero ;

                Vector3 targetPos = TargetPos;
                var direction = targetPos - _iActor.Transform.position;

                if (_param.ForwardDirection)
                {
                    float cross = direction.x * targetPos.y - direction.y * targetPos.x;
                    if (cross > 0)
                    {
                        Vector2 leftOffset = new Vector2(-direction.y, direction.x);
                        // Vector2 leftPos = _iActor.Transform.position + leftOffset * 1f;
                    }
                        
                }
                    targetPos += direction.normalized * 5f;
                    // targetPos.x += direction.x;
                    //Debug.Log(direction);

                return targetPos;
            }
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (!_isActivate)
                return;
            
            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            if (_iActor?.IStat == null)
                return;

            if (_param?.TargetICombatant != null &&
                !_param.TargetICombatant.IsActivate)
            {
                End();
                return;
            }

            Debug.DrawLine(iActorTm.position, _targetPos, Color.blue);

            if (_param != null &&
              _param.UseNavMesh)
                UpdateMovementUsingNavMesh();
            else
            {
                _targetPos = CalcTargetPos;
                UpdateMovementUsingTransform(iActorTm, _targetPos);
            }

            var direction = _prevPos - iActorTm.position;
            if (direction.x > 0)
                iActorTm.localScale = new Vector3(-1, 1, 1);
            else if (direction.x < 0)
                iActorTm.localScale = Vector3.one;

            _prevPos = iActorTm.position;

            var distance = Vector2.Distance(iActorTm.position, _targetPos);
            if (distance < _param.Distance)
            {
                Debug.Log(distance);
                // 도착 후, 현재 바라보는 방향과 반대로 바라보기.
                var localScale = iActorTm.localScale;
                localScale.x = _param.DirectionAfterArriving;
                iActorTm.localScale = localScale;

                End();
            }
        }

        private void UpdateMovementUsingNavMesh()
        {
            //Vector3 targetPos = TargetPos;
            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent == null)
                return;

            if (!navMeshAgent.enabled)
                return;

            //Debug.Log(navMeshAgent.hasPath);
            //if (!navMeshAgent.pathPending && 
            //    navMeshAgent.remainingDistance <= 0.1f)
            //{
            //    Debug.Log("타겟에 도착함");
            //    End();
            //}
        }

        private void UpdateMovementUsingTransform(Transform iActorTm, Vector3 targetPos)
        {
            var speed = _param.MoveSpeed;
            iActorTm.position = Vector2.MoveTowards(iActorTm.position, targetPos, speed * Time.deltaTime);
        }

        private void End()
        {
            _param.FinishAction?.Invoke();
            _endAction?.Invoke(_iActor);
        }
    }
}
