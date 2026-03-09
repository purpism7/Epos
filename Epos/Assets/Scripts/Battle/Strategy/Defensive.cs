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

            LeaderICombatant = iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            TryStartTraceMove(10004, DirectionType.Right, 7f, false);
            TryStartTraceMove(10003, DirectionType.Back, 7f, false);
        }

        public override async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            await base.RegroupToLeaderAsync(targetPosition, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                if (!TryStartTraceMove(10004, DirectionType.Right, 7f, true))
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
                EndTraceMove(10004);
                EndTraceMove(10003);

                UnityEngine.Debug.Log("모두 집결 완료!");
            }
        }
    }
}

