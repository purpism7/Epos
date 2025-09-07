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
using UnityEditor;
using Creature.Reaction;

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
        private Vector3 _randPos = Vector3.zero;

        private float _timeScale = 1f;
        
        public bool IsJumpMove { get { return _param != null ? _param.IsJumpMove : false; } }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // float attackSight = IStat.Get(Stat.EType.AttackSight);
            // Debug.Log(attackSight);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_randPos, 5f);
        }
#endif

        public override void Execute()
        {
            if (_param == null)
            {
                End();
                return;
            }

            _timeScale = Time.timeScale;

            Activate();
            SetAnimation(_param.AnimationKey, true);

            _targetPos = TargetPos;

            if (_param != null &&
                _param.UseNavMesh)
            {
                var navMeshAgent = _iActor?.NavMeshAgent;
                if (navMeshAgent != null)
                {
                    EnableNavMeshAgent();

                    _targetPos = CalcTargetPos;

                    SetNavMeshAgentSpeed();
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

        private void SetNavMeshAgentSpeed()
        {
            if (_param == null ||
                !_param.UseNavMesh)
                return;

            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent == null)
                return;
            //Debug.Log(Time.timeScale);
            _iActor.SkeletonAnimation.timeScale = Time.timeScale;
            navMeshAgent.speed = _param.MoveSpeed * Time.timeScale;
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
                        targetPos = _param.TargetICombatant.IActor.Transform.position;
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
                    if(_param?.LeaderTm)
                    {
                        Vector3 fromLeaderDir = (_iActor.Transform.position - _param.LeaderTm.position).normalized;
                        Vector2 toTargetDir = (_iActor.Transform.position - targetPos).normalized;

                        var crossPos = Vector3.Cross(toTargetDir, fromLeaderDir);

                        bool isLeft = crossPos.z > 0f;

                        Vector3 right = Vector3.right; // 월드 기준 오른쪽
                        Vector3 baseDir = isLeft ? right : -right; // 왼쪽 or 오른쪽 방향

                        // 반원 내 랜덤 각도 + 거리
                        float angle = Random.Range(-90f, 90f);
                        float distance = Random.Range(6f, 10f);

                        // 회전 적용
                        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                        Vector3 offset = rotation * baseDir * distance;

                        return targetPos + offset;
                    }
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
                !_param.TargetICombatant.IActor.IsActivate)
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
            else
            {
                //if (Time.timeScale <= 0)
                SetNavMeshAgentSpeed();
            }

            Debug.DrawLine(iActorTm.position, _targetPos, Color.blue);

            var direction = _prevPos - iActorTm.position;
            _iActor?.IActCtr?.Flip(direction.x);
            _iActor?.SortingOrder(iActorTm.position.y);

            _prevPos = iActorTm.position;

            var distance = Vector2.Distance(iActorTm.position, _targetPos);
            if (distance < _param.Distance)
                End();
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
            _param?.FinishAction?.Invoke();
            _endAction?.Invoke(_iActor);
        }
    }
}
