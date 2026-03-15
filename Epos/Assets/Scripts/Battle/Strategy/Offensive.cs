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

            LeaderCombatant = strategyDataProvider?.Allycombatants?.Find(combatant => combatant.Actor.Id == 10004);
        }

        public override void CleanupTraceCallbacks()
        {
            EndTraceMove(10001);
            EndTraceMove(10003);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            TryStartTraceTo(10001, DirectionType.Left, 6f, false);
            TryStartTraceTo(10003, DirectionType.Back, 6f, false);
        }

        public override async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await base.RegroupToLeaderAsync(targetPosition, cancellationToken);
            
            try
            {
                if (!TryStartTraceTo(10001, DirectionType.Left, 6f, true))
                    return;

                if (!TryStartTraceTo(10003, DirectionType.Back, 6f, true))
                    return;

                if (_totalMoveCount > 0)
                    await UniTask.WaitUntil(() => _completedCount >= _totalMoveCount, cancellationToken: cancellationToken)
                        .Timeout(TimeSpan.FromSeconds(3f));
            }
            catch (OperationCanceledException)
            {
                // 취소 시 깔끔하게 종료
            }
            finally
            {
                CleanupTraceCallbacks();

                UnityEngine.Debug.Log("모두 집결 완료!");
            }
        }
    }
}

