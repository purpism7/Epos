using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

using Common;
using Creature;
using Creature.Action;
using GameSystem;

namespace Battle.Strategy
{
    public class Defensive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            base.Apply(iStrategyDataProvider);

            LeaderCombatant = iStrategyDataProvider?.Allycombatants?.Find(combatant => combatant.Actor.Id == 10001);
        }

        public override void CleanupTraceCallbacks()
        {
            EndTraceMove(10004);
            EndTraceMove(10003);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            TryStartTraceTo(10004, DirectionType.Right, 6f, false);
            TryStartTraceTo(10003, DirectionType.Back, 6f, false);
        }

        public override async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await base.RegroupToLeaderAsync(targetPosition, cancellationToken);

            try
            {
                if (!TryStartTraceTo(10004, DirectionType.Right, 6f, true))
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

