using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static UnityEngine.UI.Image;

using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;

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
            public Transform LeaderTm { get; private set; } = null;
            public bool ForwardDirection { get; private set; } = false;
            public float Distance { get; private set; } = 0.1f;

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

            public Param WithLeaderTm(Transform leaderTm)
            {
                LeaderTm = leaderTm;
                return this;
            }

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
            {
                End();
                return;
            }
            
            Activate();
            SetAnimation(_param.AnimationKey, true);

            _targetPos = TargetPos;
            
            if (_param != null &&
                _param.UseNavMesh)
            {
                var navMeshAgent = _iActor?.NavMeshAgent;
                if (navMeshAgent != null )
                {
                    EnableNavMeshAgent();

                    _targetPos = CalcTargetPos;

                    navMeshAgent.speed = _param.MoveSpeed;
                    navMeshAgent.SetDestination(_targetPos);
                }
            }
            else
                DisableNavMeshAgent();

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

            DisableNavMeshAgent();
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

        private void DisableNavMeshAgent()
        {
            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent != null &&
                navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }
        }

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

                if (_param.ForwardDirection)
                {
                    var randPos = targetPos + Random.insideUnitSphere.normalized * 5f;

                    Vector3 direction = (_iActor.Transform.position - _param.LeaderTm.position).normalized;
                    Vector2 desiredVelocity = (_iActor.Transform.position - targetPos).normalized;
                     
                    // var direction = targetPos - _iActor.Transform.position;

                    var crossPos = Vector3.Cross(desiredVelocity, direction);
                    
                    //float crossZ = direction.x * _iActor.Transform.position.y - direction.y * _iActor.Transform.position.x;
                    bool isLeft = crossPos.z > 0f;
                    //bool isRight = crossZ < 0f;
                    // Debug.Log(_iActor.Id + " = " + isLeft);

                    //Debug.Log(isLeft + " / " + isRight);
                    return randPos;
                }

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

            //if (_param != null &&
            //  _param.UseNavMesh)
            //    UpdateMovementUsingNavMesh();
            if(_param != null &&
              !_param.UseNavMesh)
            {
                _targetPos = TargetPos;
                UpdateMovementUsingTransform(iActorTm, _targetPos);
            }

            Debug.DrawLine(iActorTm.position, _targetPos, Color.blue);

            var direction = _prevPos - iActorTm.position;
            _iActor?.IActCtr?.Flip(direction.x);
            _iActor?.SortingOrder(iActorTm.position.y);

            _prevPos = iActorTm.position;

            var distance = Vector2.Distance(iActorTm.position, _targetPos);
            if (distance < _param.Distance)
            {
                // 도착 후, 현재 바라보는 방향과 반대로 바라보기.
                // var localScale = iActorTm.localScale;
                // localScale.x = _param.DirectionAfterArriving;
                // iActorTm.localScale = localScale;

                End();
            }
        }

        //private void UpdateMovementUsingNavMesh()
        //{
        //    //Vector3 targetPos = TargetPos;
        //    var navMeshAgent = _iActor?.NavMeshAgent;
        //    if (navMeshAgent == null)
        //        return;

        //    if (!navMeshAgent.enabled)
        //        return;

        //    //Debug.Log(navMeshAgent.hasPath);
        //    //if (!navMeshAgent.pathPending && 
        //    //    navMeshAgent.remainingDistance <= 0.1f)
        //    //{
        //    //    Debug.Log("타겟에 도착함");
        //    //    End();
        //    //}
        //}

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
