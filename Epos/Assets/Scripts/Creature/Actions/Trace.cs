using UnityEngine;
using UnityEngine.AI;

using Common;
using TMPro;

namespace Creature.Actions
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
        // 처음 시작할 때 튈 수 있으니, 기본값은 위쪽(혹은 리더가 바라보는 방향)으로 줍니다.
        private Vector3 _lastValidDirection = Vector3.zero;

        public override void Execute()
        {
            if (_param == null)
                return;

            if (_param.TargetTm)
                _prevTargetPosition = _param.TargetTm.position;
            else if (_param.TargetICombatant != null)
                _prevTargetPosition = _param.TargetICombatant.Transform.position;
            
            Activate();

            if(_param.Distance > 0)
            {
                var distance = Vector2.Distance(_actor.Transform.position, TargetPosition);
                if(distance > _param.Distance)
                    _param.WithSpeed(_param.Speed + 1f);
            }

            PlayAnimation(_param.AnimationKey, true);
            
            _actor?.EffectController?.Activate("Eff_run_01", new Effect.Param().WithTargetSkeletonAnimation(_actor?.SkeletonAnimation), "Move");
        }

        protected override void Activate()
        {
            base.Activate();
            
            EnableNavMeshAgent();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            DisableNavMeshAgent();
        }

        private void EnableNavMeshAgent()
        {
            var navMeshAgent = _actor?.NavMeshAgent;
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;
            }
        }

        private void DisableNavMeshAgent()
        {
            var navMeshAgent = _actor?.NavMeshAgent;
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

            var navMeshAgent = _actor?.NavMeshAgent;
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
                }

                targetPosition = GetTargetPositionByDirection(targetPosition);

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
            float sqrMag = directionToTarget.sqrMagnitude;

            if (sqrMag >= 0.0001f)
            {
                Vector3 newDir = directionToTarget.normalized;
                // 전술 변경 후 MoveFormation: 리더가 꺾을 때마다 방향이 매 프레임 바뀌어 캐릭터끼리 뱅뱅 도는 현상 방지
                if (_lastValidDirection.sqrMagnitude < 0.01f)
                    _lastValidDirection = newDir;
                else
                    _lastValidDirection = Vector3.Slerp(_lastValidDirection, newDir, 0.2f).normalized;
            }
            else if (_lastValidDirection == Vector3.zero)
            {
                _lastValidDirection = _targetTm.up;
            }

            // 2. 방향 타입에 따른 오프셋 계산 (스무딩된 _lastValidDirection 사용)
            Vector3 offset = Vector3.zero;
            Vector3 dir = _lastValidDirection;

            switch (_param.DirectionType)
            {
                case DirectionType.Forward:
                    {
                        offset = dir * _param.Distance;
                        break;
                    }

                case DirectionType.Back:
                    {
                        // 수정: directionToTarget 대신 dir 사용!
                        offset = -dir * _param.Distance;
                        break;
                    }

                case DirectionType.Right:
                    {
                        // 수정: directionToTarget 대신 dir 사용!
                        offset = new Vector3(dir.y, -dir.x, 0) * _param.Distance;
                        break;
                    }

                case DirectionType.Left:
                    {
                        // 수정: directionToTarget 대신 dir 사용!
                        offset = new Vector3(-dir.y, dir.x, 0) * _param.Distance;
                        break;
                    }
            }

            // 3. 기존의 Z값을 유지하면서 오프셋 적용
            float originalZ = targetPosition.z;
            targetPosition += offset;
            targetPosition.z = originalZ;

            return targetPosition;
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (!_isActivate)
                return;

            if (_param == null || !_actor?.Transform || _actor?.NavMeshAgent == null)
                return;

            var navMeshAgent = _actor.NavMeshAgent;

            // targetPosition은 '리더의 위치'가 아니라 '이미 오프셋이 적용된 내 최종 목적지'입니다.
            Vector3 targetPosition = TargetPosition;
            Vector3 actorPosition = _actor.Transform.position;

            // 리더의 동선(과거 위치) 갱신은 내가 멈춰있든 말든 매 프레임 무조건 해줍니다! (방향 꼬임 방지)
            if (_targetTm != null)
            {
                _prevTargetPosition = _targetTm.position;
            }

            var distance = Vector2.Distance(actorPosition, targetPosition);
            if (distance <= 0.1f)
            {
                if (_param.IsEndOnArrival)
                {
                    End();
                    return;
                }

                // 계속 따라다니는 상태라면? 브레이크를 확실히 밟아줍니다. (제자리 맴도는 것 방지)
                if (!navMeshAgent.isStopped)
                {
                    navMeshAgent.isStopped = true;
                    navMeshAgent.velocity = Vector3.zero;
                    _actor?.EffectController?.Deactivate("Move");
                }
            }
            else
            {
                // 리더가 도망가서 거리가 벌어졌다면 다시 쫓아갑니다!
                if (navMeshAgent.isStopped)
                {
                    navMeshAgent.isStopped = false;
                    _actor?.EffectController?.Activate("Eff_run_01", new Effect.Param().WithTargetSkeletonAnimation(_actor?.SkeletonAnimation), "Move");
                }

                SetNavMeshAgentSpeed();
                navMeshAgent.SetDestination(targetPosition);

                var direction = targetPosition - actorPosition;
                if (direction.sqrMagnitude > 0.001f) // 너무 미세한 진동 시엔 안 쳐다보게 방어
                {
                    _actor?.ActController?.Flip(direction.x);
                }
            }

            _actor?.SetSortingOrder(actorPosition.y);
        }

        protected override void End()
        {
            base.End();
            
            _actor?.EffectController?.Deactivate("Move");
        }
    }
}
