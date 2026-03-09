using Common;
using Creature;
using Creature.Action;
using Cysharp.Threading.Tasks;
using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Battle.Strategy
{
    public interface IStrategy
    {
        void Apply(IStrategyDataProvider iStrategyDataProvider);

        void ChainUpdate();

        void InitializeFormationPosition();
        void MoveFormation(Vector3 targetPosition);
        UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken);

        ICombatant LeaderICombatant { get; }
}

    public abstract class BaseStrategy : IStrategy
    {
        protected IStrategyDataProvider _strategyDataProvider = null;
        protected List<ICombatant> _combatants = new List<ICombatant>();
        protected int _totalMoveCount = 0;
        protected int _completedCount = 0;

#if UNITY_EDITOR
        private DebugObject _debugObject = null;
#endif

        public ICombatant LeaderICombatant { get; protected set; } = null;

        public virtual void Apply(IStrategyDataProvider strategyDataProvider)
        { 
            _strategyDataProvider = strategyDataProvider;
            _totalMoveCount = 0;
            _completedCount = 0;
        }

        public virtual void ChainUpdate()
        {
            
        }

        public virtual void InitializeFormationPosition()
        {
            
        }

        public virtual void MoveFormation(Vector3 targetPosition)
        {
            if (LeaderICombatant == null)
                return;

            var actorCtr = LeaderICombatant?.IActor?.IActCtr;
            if (actorCtr == null)
                return;

            MoveLeaderToTarget(targetPosition);

#if UNITY_EDITOR
            if(!_debugObject)
            {
                var debugGameObj= new GameObject();
                _debugObject = debugGameObj.AddComponent<DebugObject>();
            }

            _debugObject.originTm = LeaderICombatant.Transform;
            _debugObject.targetPosition = targetPosition;
#endif
        }

        public virtual async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            if (targetPosition != null)
            {
                MoveLeaderToTarget(targetPosition.Value);

                await UniTask.Yield();
            }

            Debug.Log("RegroupToLeaderAsync");
        }

        private void MoveLeaderToTarget(Vector3 targetPosition)
        {
            if (LeaderICombatant == null)
                return;

            var actorCtr = LeaderICombatant?.IActor?.IActCtr;
            if (actorCtr == null)
                return;

            var moveSpeed = LeaderICombatant.IStat.Get(Stat.EType.MoveSpeed);
            var moveParam = new Move.Param
            {
                MoveSpeed = moveSpeed,
                TargetPos = targetPosition,
            };

            actorCtr.MoveTo(moveParam)?.Execute();
        }

        protected void TraceTo(ICombatant combatant, DirectionType directionType, float distance, bool isEndOnArrival)
        {
            if (combatant == null)
                return;

            var moveSpeed = combatant.IStat.Get(Stat.EType.MoveSpeed);

            var traceParam = new Creature.Action.Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(directionType)
                .WithDistance(distance)
                .WithSpeed(moveSpeed)
                .WithEndOnArrival(isEndOnArrival);

            combatant.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }

        protected bool TryStartTraceMove(int characterId, DirectionType directionType, float distance, bool isEndOnArrival)
        {
            var combatant = _strategyDataProvider?.AllyICombatantList?.Find(c => c.IActor.Id == characterId);
            if (combatant?.IActor?.IActCtr != null)
            {
                TraceTo(combatant, directionType, distance, isEndOnArrival);
                combatant.IActor?.IActCtr?.OnActEnded<Creature.Action.Trace>(OnTraceActionEnded);

                _totalMoveCount++;

                return true;
            }

            return false;
        }

        protected void EndTraceMove(int characterId)
        {
            var combatant = _strategyDataProvider?.AllyICombatantList?.Find(c => c.IActor.Id == characterId);
            combatant?.IActor?.IActCtr?.RemoveActEnded<Creature.Action.Trace>(OnTraceActionEnded);
        }

        private void OnTraceActionEnded(Creature.Action.Trace act)
        {
            _completedCount++;
        }

        protected void SetFormationPosition(ICombatant iCombatant, DirectionType directionType, float distance, Vector2 offsetPosition)
        {
            var targetPosition = LeaderICombatant.Transform.position;
            Vector2 targetDirection = LeaderICombatant.Transform.up; 
    
            Vector2 normalizedDirection = targetDirection.normalized;

            var resPosition = Vector3.zero;
            switch (directionType)
            {
                case DirectionType.Forward:
                {
                    // Forward: 리더 전방 방향으로 배치
                    resPosition = targetPosition + (Vector3)normalizedDirection * distance;
                    break;
                }
                
                case DirectionType.Back:
                {
                    // Back: 타겟 전방 벡터를 반대 방향으로 사용
                    resPosition = targetPosition - (Vector3)normalizedDirection * distance;
                    break;
                }
                
                case DirectionType.Right:
                {
                    // Right Vector (90도 시계 방향 회전): (y, -x)
                    Vector2 rightVector = new Vector2(normalizedDirection.y, -normalizedDirection.x);
                    resPosition = targetPosition + ((Vector3)rightVector * distance);
                    break;
                }
        
                case DirectionType.Left:
                {
                    // Left Vector (90도 반시계 방향 회전): (-y, x)
                    Vector2 leftVector = new Vector2(-normalizedDirection.y, normalizedDirection.x);
                    resPosition = targetPosition + ((Vector3)leftVector * distance);
                    break;
                }
            }

            resPosition.x += offsetPosition.x;
            resPosition.y += offsetPosition.y;

            iCombatant?.SetPosition(resPosition);
        }
    }
}
