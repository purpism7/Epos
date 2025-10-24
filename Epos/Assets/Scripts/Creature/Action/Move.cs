using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Cysharp.Threading.Tasks;
using VContainer;

using Entities;
using Spine;

using static UnityEngine.UI.Image;

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

            public Param WithUseNavMesh(bool useNavMesh)
            {
                UseNavMesh = useNavMesh;
                return this;
            }

            public Param WithDistance(float distance)
            {
                Distance = distance;
                return this;
            }
        }

        private const string StopAnimationName = "Stop";

        private Vector3 _prevPos = Vector3.zero;
        private Vector3 _randPos = Vector3.zero;

        private float _totalDistance = 0;
        private bool _isEnded = false;
        
        public bool IsJumpMove { get { return _param != null ? _param.IsJumpMove : false; } }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // float attackSight = IStat.Get(Stat.EType.AttackSight);
            // Debug.Log(attackSight);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_randPos, 5f);

            Handles.color = new Color(0f, 1f, 0f, 0.2f);
        }
#endif

        public override void Execute()
        {
            if (_param == null)
            {
                End();
                return;
            }

            Activate();
            PlayAnimation(_param.AnimationKey, true);

            _totalDistance = 0;
            _isEnded = false;

            if (_param != null &&
                _param.UseNavMesh)
            {
                var navMeshAgent = _iActor?.NavMeshAgent;
                if (navMeshAgent != null)
                {
                    EnableNavMeshAgent();
                    SetNavMeshAgentSpeed();
                }
            }
            else
                DisableNavMeshAgent();

            //if (_iActor?.Transform)
            //    _prevPos = _iActor.Transform.position;

            _iActor?.IEffectCtr?.Activate(GetType().Name, new Effect.Param().WithTargetSkeletonAnimation(_iActor?.SkeletonAnimation), "Eff_run_01");
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
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.enabled = false;
            }
        }

        private void SetNavMeshAgentSpeed()
        {
            if (_param == null)
                return;

            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent == null)
                return;

            if (navMeshAgent.speed == _param.MoveSpeed)
                return;

            navMeshAgent.speed = _param.MoveSpeed; // * Time.timeScale;
        }

        private Vector3 TargetPos
        {
            get
            {
                Vector3 targetPos = Vector3.zero;
                Transform targetTm = null;

                if(_param != null)
                {
                    if (_param.TargetTm)
                    {
                        targetTm = _param.TargetTm;
                        targetPos = _param.TargetTm.position;
                    }
                        
                    if (_param.TargetPos != null)
                        targetPos = _param.TargetPos.Value;

                    if (_param.TargetICombatant != null)
                    {
                        targetTm = _param.TargetICombatant.Transform;
                        targetPos = _param.TargetICombatant.Transform.position;
                        
                        var targetCollider = _param.TargetICombatant.IActor?.Collider;
                        if (targetCollider != null)
                        {
                            var closesetPosition = targetCollider.ClosestPoint(_iActor.Transform.position);
                            targetPos = closesetPosition;
                        }
                    }
                }

                if(targetTm)
                {
                    // 1. 목표의 양 옆 위치를 정의합니다.
                    // target.right는 2D 공간의 오른쪽 방향 벡터 (Vector2)로 자동 변환됩니다.
                    Vector2 rightPos = (Vector2)targetTm.position + ((Vector2)targetTm.right);
                    Vector2 leftPos = (Vector2)targetTm.position - ((Vector2)targetTm.right);

                    // 2. 공격자와 양 옆 위치까지의 거리를 계산합니다.
                    float distanceToRight = Vector2.Distance(_iActor.Transform.position, rightPos);
                    float distanceToLeft = Vector2.Distance(_iActor.Transform.position, leftPos);

                    // 3. 거리를 비교하여 더 가까운 지점을 선택합니다.
                    targetPos = (distanceToRight < distanceToLeft) ? rightPos : leftPos;
                }


                return targetPos;
            }
        }

        //private Vector3 CalcTargetPos
        //{
        //    get
        //    {
        //        if (_iActor == null)
        //            return Vector3.zero ;

        //        Vector3 targetPos = TargetPos;

        //        if (_param.ForwardDirection)
        //        {
        //            //if(_param?.LeaderTm)
        //            //{
        //            //Vector3 fromLeaderDir = (_iActor.Transform.position - _param.LeaderTm.position).normalized;
        //            //Vector2 toTargetDir = (_iActor.Transform.position - targetPos).normalized;

        //            //var crossPos = Vector3.Cross(toTargetDir, fromLeaderDir);

        //            //bool isLeft = crossPos.z > 0f;

        //            //    Vector3 right = Vector3.right; // 월드 기준 오른쪽
        //            //    Vector3 baseDir = isLeft ? right : -right; // 왼쪽 or 오른쪽 방향

        //            //    // 반원 내 랜덤 각도 + 거리
        //            //    float angle = Random.Range(-90f, 90f);
        //            //    float distance = Random.Range(6f, 10f);

        //            //    // 회전 적용
        //            //    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        //            //    Vector3 offset = rotation * baseDir * distance;

        //            //    return targetPos + offset;
        //            //}
        //        }

        //        return targetPos;
        //    }
        //}

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (!_isActivate)
                return;

            if (_isEnded)
                return;

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            if (_iActor?.IStat == null)
                return;

            var target = _param.TargetICombatant;
            if (target != null)
            {
                var targetIActor = target.IActor;
                if (targetIActor != null &&
                    !targetIActor.IsAlive)
                {
                    End();
                    return;
                }
            }

            var targetPosition = TargetPos;

            if (_param != null &&
              !_param.UseNavMesh)
            {
                UpdateMovementUsingTransform(iActorTm, targetPosition);
            }
            else
            {
                SetNavMeshAgentSpeed();
                _iActor.NavMeshAgent?.SetDestination(targetPosition);
            }

            Debug.DrawLine(iActorTm.position, targetPosition, Color.blue);

            var direction = targetPosition - iActorTm.position;

            _iActor?.IActCtr?.Flip(direction.x);
            _iActor?.SortingOrder(iActorTm.position.y);

            //_prevPos = iActorTm.position;

            var distance = Vector2.Distance(iActorTm.position, targetPosition);
            _totalDistance += distance;
            //Debug.Log("_totalDistance  = " + _totalDistance);

            if (distance < _param.Distance)
                End();
        }

        private void UpdateMovementUsingTransform(Transform iActorTm, Vector3 targetPos)
        {
            var speed = _param.MoveSpeed;
            iActorTm.position = Vector2.MoveTowards(iActorTm.position, targetPos, speed * Time.deltaTime);
        }

        protected override void End()
        {
            if (_isEnded)
                return;

            _iActor?.IEffectCtr?.Deactivate(GetType().Name);

            _isEnded = true;
            _param?.FinishAction?.Invoke();

            base.End();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);

            var animation = trackEntry?.Animation;
            if(animation != null)
            {
                if(animation.Name == StopAnimationName)
                    _param?.FinishAction?.Invoke();
            }
        }
    }
}
