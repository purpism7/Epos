using Common;
using Creature;
using Creature.Action;
using Cysharp.Threading.Tasks;
using GameSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Battle.Strategy
{
    public class Adaptive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            base.Apply(iStrategyDataProvider);
    
            LeaderICombatant = iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
        }

        public override void InitializeFormationPosition()
        {
            base.InitializeFormationPosition();
            
            var iCombatant = _strategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            SetFormationPosition(iCombatant, DirectionType.Right, 7f, Vector2.zero);
            
            iCombatant = _strategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
            SetFormationPosition(iCombatant, DirectionType.Forward, 6f, new Vector2(5f, 0));
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            TryStartTraceMove(10004, DirectionType.Back, 7f, false);
            TryStartTraceMove(10001, DirectionType.Forward, 7f, false);
        }

        public override async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            await base.RegroupToLeaderAsync(targetPosition, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                if (!TryStartTraceMove(10004, DirectionType.Back, 7f, true))
                    return;

                if (!TryStartTraceMove(10001, DirectionType.Forward, 7f, true))
                    return;

                await UniTask.WaitUntil(() => _completedCount >= _totalMoveCount, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 취소 시 깔끔하게 종료
            }
            finally
            {
                EndTraceMove(10004);
                EndTraceMove(10001);

                UnityEngine.Debug.Log("모두 집결 완료!");
            }
        }
    }
}