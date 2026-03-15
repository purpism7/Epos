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

        /// <summary>
        /// 전략 변경 시 이전 전략이 등록한 Trace 콜백을 제거합니다.
        /// </summary>
        void CleanupTraceCallbacks();

        ICombatant LeaderCombatant { get; }
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

        public ICombatant LeaderCombatant { get; protected set; } = null;

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

        public virtual void CleanupTraceCallbacks()
        {

        }

        public virtual void MoveFormation(Vector3 targetPosition)
        {
            if (LeaderCombatant == null)
                return;

            MoveLeaderToTarget(targetPosition);

//#if UNITY_EDITOR
//            if(!_debugObject)
//            {
//                var debugGameObj= new GameObject();
//                _debugObject = debugGameObj.AddComponent<DebugObject>();
//            }

//            _debugObject.originTm = LeaderICombatant.Transform;
//            _debugObject.targetPosition = targetPosition;
//#endif
        }

        public virtual UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            _totalMoveCount = 0;
            _completedCount = 0;

            if (targetPosition != null)
                MoveLeaderToTarget(targetPosition.Value);

            Debug.Log("RegroupToLeaderAsync");
            return UniTask.CompletedTask;
        }

        private void MoveLeaderToTarget(Vector3 targetPosition)
        {
            if (LeaderCombatant == null)
                return;

            var actController = LeaderCombatant?.Actor?.ActController;
            if (actController == null)
                return;

            var moveSpeed = LeaderCombatant.IStat.Get(Stat.EType.MoveSpeed);
            var moveParam = new Move.Param
            {
                MoveSpeed = moveSpeed,
                TargetPos = targetPosition,
            };

            actController.MoveTo(moveParam)?.Execute();
        }

        protected void TraceTo(ICombatant combatant, DirectionType directionType, float distance, bool isEndOnArrival)
        {
            if (combatant == null)
                return;

            var moveSpeed = combatant.IStat.Get(Stat.EType.MoveSpeed);

            var traceParam = new Creature.Action.Trace.Param()
                .WithTargetICombatant(LeaderCombatant)
                .WithDirectionType(directionType)
                .WithDistance(distance)
                .WithSpeed(moveSpeed)
                .WithEndOnArrival(isEndOnArrival);

            combatant.Actor?.ActController?
                .TraceTo(traceParam)?
                .Execute();
        }

        protected bool TryStartTraceTo(int characterId, DirectionType directionType, float distance, bool isEndOnArrival)
        {
            var combatant = _strategyDataProvider?.Allycombatants?.Find(c => c.Actor.Id == characterId);
            if (combatant?.Actor?.ActController != null)
            {
                TraceTo(combatant, directionType, distance, isEndOnArrival);

                if(isEndOnArrival)
                {
                    combatant.Actor.ActController.OnActEnded<Creature.Action.Trace>(OnTraceActionEnded);
                    ++_totalMoveCount;
                }

                return true;
            }

            return false;
        }

        protected void EndTraceMove(int characterId)
        {
            var combatant = _strategyDataProvider?.Allycombatants?.Find(c => c.Actor.Id == characterId);
            combatant?.Actor?.ActController?.RemoveActEnded<Creature.Action.Trace>(OnTraceActionEnded);
        }

        private void OnTraceActionEnded(Creature.Action.Trace act)
        {
            ++_completedCount;
        }

        protected void SetFormationPosition(ICombatant iCombatant, DirectionType directionType, float distance, Vector2 offsetPosition)
        {
            var targetPosition = LeaderCombatant.Transform.position;
            Vector2 targetDirection = LeaderCombatant.Transform.up; 
    
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
