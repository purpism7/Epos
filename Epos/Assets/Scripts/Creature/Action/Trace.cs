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
            public bool IsLeft { get; private set; } = false;

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

            public Param WithIsLeft(bool isLeft)
            {
                IsLeft = isLeft;
                return this;
            }
        }

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
                navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
            }
        }

        private void DisableNavMeshAgent()
        {
            var navMeshAgent = _iActor?.NavMeshAgent;
            if (navMeshAgent != null &&
                navMeshAgent.enabled)
            {
                navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.MedQualityObstacleAvoidance;
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

            if (navMeshAgent.speed == _param.Speed)
                return;

            navMeshAgent.speed = _param.Speed; // * Time.timeScale;
        }

        private Vector2 TargetPosition
        {
            get
            {
                Vector3 targetPosition = Vector3.zero;
                Transform targetTm = null;

                if(_param?.TargetTm)
                {
                    targetPosition = _param.TargetTm.position;
                    targetTm = _param?.TargetTm;
                }
                    
                if (_param?.TargetICombatant != null)
                {
                    targetPosition = _param.TargetICombatant.Transform.position;
                    targetTm = _param.TargetICombatant.Transform;

                    var targetCollider = _param.TargetICombatant.IActor?.Collider;
                    if (targetCollider != null)
                    {
                        var closesetPosition = targetCollider.ClosestPoint(_iActor.Transform.position);
                        targetPosition = closesetPosition;
                    }
                }

                if(_param.IsLeft)
                    targetPosition += -(targetTm.right * 2f);
                else
                    targetPosition += targetTm.right * 2f;

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

            Debug.DrawLine(iActorTm.position, targetPosition, Color.yellow);

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
