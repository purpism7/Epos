using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

using Common;
using Creature;
using Creature.Action;
using GameSystem;
using System.Threading;

namespace Battle.Strategy
{
    public class Offensive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider strategyDataProvider)
        {
            base.Apply(strategyDataProvider);
    
            LeaderICombatant = strategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            TryStartTraceMove(10001, DirectionType.Left, 7f, false);
            TryStartTraceMove(10003, DirectionType.Back, 7f, false);
        }

        public override async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            await base.RegroupToLeaderAsync(targetPosition, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                if (!TryStartTraceMove(10001, DirectionType.Left, 7f, true))
                    return;

                if (!TryStartTraceMove(10003, DirectionType.Back, 7f, true))
                    return;

                await UniTask.WaitUntil(() => _completedCount >= _totalMoveCount, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 취소 시 깔끔하게 종료
            }
            finally
            {
                EndTraceMove(10001);
                EndTraceMove(10003);

                UnityEngine.Debug.Log("모두 집결 완료!");
            }
        }
    }
}

