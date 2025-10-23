using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;

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
                Speed = speed;
                return this;
            }

            public Param WithDistance(float distance)
            {
                Distance = distance;
                return this;
            }
        }

        private Vector3 _prevPosition = Vector3.zero;

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
                    _param?.WithSpeed(_param.Speed + 1f);
            }

            PlayAnimation(_param.AnimationKey, true);
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
                navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.MedQualityObstacleAvoidance;
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
                navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            }
        }

        private void SetNavMeshAgentSpeed()
        {
            if (_param == null)
                return;

            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent == null)
                return;

            if (navMeshAgent.speed == _param.Speed)
                return;

            navMeshAgent.speed = _param.Speed; // * Time.timeScale;
        }

        private Vector2 TargetPosition
        {
            get
            {
                Vector3 targetPosition = Vector3.zero;

                if(_param?.TargetTm)
                    targetPosition = _param.TargetTm.position;

                if (_param?.TargetICombatant != null)
                {
                    targetPosition = _param.TargetICombatant.Transform.position;

                    var targetCollider = _param.TargetICombatant.IActor?.Collider;
                    if (targetCollider != null)
                    {
                        var closesetPosition = targetCollider.ClosestPoint(_iActor.Transform.position);
                        targetPosition = closesetPosition;
                    }
                }

                return targetPosition;
            }
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
            var distance = Vector2.Distance(iActorTm.position, targetPosition);
            if (distance < _param.Distance)
                return;

            SetNavMeshAgentSpeed();
            navMeshAgent.SetDestination(targetPosition);

            var direction = targetPosition - iActorTm.position;

            _iActor?.IActCtr?.Flip(direction.x);
            _iActor?.SortingOrder(iActorTm.position.y);

            //_prevPosition = iActorTm.position;

            distance = Vector2.Distance(iActorTm.position, targetPosition);
            if (distance < _param.Distance)
                End();
        }
    }
}
