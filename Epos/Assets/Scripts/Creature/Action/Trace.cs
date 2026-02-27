using UnityEngine;
using UnityEngine.AI;

using Common;
using TMPro;

namespace Creature.Action
{
    public class Trace : Act<Trace.Param>
    {
        public class Param : ActParam
        {
            public Transform TargetTm { get; private set; } = null;
            public ICombatant TargetICombatant { get; private set; } = null;

            public float Speed { get; private set; } = 1f;
            public float Distance { get; private set; } = 0;
            public DirectionType DirectionType { get; private set; } = DirectionType.None;
            public bool IsEndOnArrival { get; private set; } = true;

            public Param WithTargetTransform(Transform targetTm)
            {
                TargetTm = targetTm;
                return this;
            }

            public Param WithTargetICombatant(ICombatant targetICombatant)
            {
                TargetICombatant = targetICombatant;
                return this;
            }

            public Param WithSpeed(float speed)
            {
                if (speed <= 0)
                    speed = 1f;
                
                Speed = speed;
                return this;
            }

            public Param WithDistance(float distance)
            {
                Distance = distance;
                return this;
            }

            public Param WithDirectionType(DirectionType directionType)
            {
                DirectionType = directionType;
                return this;
            }

            public Param WithEndOnArrival(bool isEndOnArrival)
            {
                IsEndOnArrival = isEndOnArrival;
                return this;
            }
        }

        private Transform _targetTm = null;
        private Vector3 _prevTargetPosition = Vector3.zero;

        public override void Execute()
        {
            if (_param == null)
                return;

            EnableNavMeshAgent();
            Activate();

            if(_param.Distance > 0)
            {
                var distance = Vector2.Distance(_iActor.Transform.position, TargetPosition);
                if(distance > _param.Distance)
                    _param.WithSpeed(_param.Speed + 1f);
            }

            PlayAnimation(_param.AnimationKey, true);
            
            _iActor?.IEffectCtr?.Activate("Eff_run_01", new Effect.Param().WithTargetSkeletonAnimation(_iActor?.SkeletonAnimation), "Move");
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

            //if (navMeshAgent.speed < _param.Speed)
            //    return;

            navMeshAgent.speed = _param.Speed; // * Time.timeScale;
        }

        private Vector3 TargetPosition
        {
            get
            {
                if (_param == null)
                    return Vector2.zero;
                
                Vector3 targetPosition = Vector3.zero;

                if(_param.TargetTm)
                {
                    targetPosition = _param.TargetTm.position;
                    _targetTm = _param.TargetTm;
                }
                    
                if (_param.TargetICombatant != null)
                {
                    targetPosition = _param.TargetICombatant.Transform.position;
                    _targetTm = _param.TargetICombatant.Transform;

                    //var targetCollider = _param.TargetICombatant.IActor?.Collider;
                    //if (targetCollider != null)
                    //    targetPosition = targetCollider.ClosestPoint(_iActor.Transform.position);
                }

                targetPosition = GetTargetPositionByDirection(targetPosition);

                //NavMeshHit hit;
                //// 2. 그 위치 근처(1.0f 반경)에 NavMesh(땅)가 있는지 확인
                //// SamplePosition은 가장 가까운 유효한 땅 좌표를 hit.position에 담아줍니다.
                //if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
                //    targetPosition = hit.position;

                return targetPosition;
            }
        }

        Vector3 GetTargetPositionByDirection(Vector3 targetPosition)
        {
            if(_param == null)
                return targetPosition;

            if (_param.DirectionType == DirectionType.None)
                return targetPosition;

            Vector3 directionToTarget = _targetTm.position - _prevTargetPosition;

            //if (directionToTarget.sqrMagnitude >= 0.0001f)
            //    directionToTarget = directionToTarget.normalized;

            if (directionToTarget.sqrMagnitude < 0.0001f)
            {
                // 2D 게임이면 Up(Y축)이나 Right(X축)를 기본 방향으로 설정
                directionToTarget = _targetTm.up; // 혹은 right
            }
            else
            {
                directionToTarget.Normalize();
            }

            switch (_param.DirectionType)
            {
                case DirectionType.Back:
                    {
                        targetPosition -= directionToTarget * _param.Distance;
                        break;
                    }
                
                case DirectionType.Right:
                    {
                        Vector2 rightVector = new Vector2(directionToTarget.y, -directionToTarget.x);
                        targetPosition += ((Vector3)rightVector * _param.Distance);
                        break;
                    }
                
                case DirectionType.Left:
                    {
                        Vector2 leftVector = new Vector2(-directionToTarget.y, directionToTarget.x);
                        targetPosition += ((Vector3)leftVector * _param.Distance);
                        break;
                    }
            }

            return targetPosition;
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (_param == null)
                return;

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent == null)
                return;

            Vector3 targetPosition = TargetPosition;
            Vector3 iActorPosition = iActorTm.position;

            var distance = Vector2.Distance(iActorPosition, targetPosition);
            if (distance <= _param.Distance + 0.05f)
            {
                if (_param.IsEndOnArrival)
                    End();

                return;
            }

            SetNavMeshAgentSpeed();
            navMeshAgent.SetDestination(targetPosition);

            Debug.DrawLine(iActorPosition, targetPosition, Color.magenta);

            var direction = targetPosition - iActorPosition;

            _iActor?.IActCtr?.Flip(direction.x);
            _iActor?.SortingOrder(iActorPosition.y);

            _prevTargetPosition = _targetTm.position;

            //var distance = Vector2.Distance(iActorTm.position, targetPosition);
            //if (distance < navMeshAgent.stoppingDistance)
            //    End();
        }

        protected override void End()
        {
            base.End();
            
            _iActor?.IEffectCtr?.Deactivate("Move");
        }
    }
}
