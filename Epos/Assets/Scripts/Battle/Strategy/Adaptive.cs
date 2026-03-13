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
        public override void Apply(IStrategyDataProvider strategyDataProvider)
        {
            base.Apply(strategyDataProvider);

            LeaderCombatant = strategyDataProvider?.Allycombatants?.Find(combatant => combatant.Actor.Id == 10003);
        }

        public override void InitializeFormationPosition()
        {
            base.InitializeFormationPosition();
            
            var iCombatant = _strategyDataProvider?.Allycombatants?.Find(combatant => combatant.Actor.Id == 10004);
            SetFormationPosition(iCombatant, DirectionType.Right, 7f, Vector2.zero);
            
            iCombatant = _strategyDataProvider?.Allycombatants?.Find(combatant => combatant.Actor.Id == 10001);
            SetFormationPosition(iCombatant, DirectionType.Forward, 6f, new Vector2(5f, 0));
        }

        public override void CleanupTraceCallbacks()
        {
            EndTraceMove(10004);
            EndTraceMove(10001);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            TryStartTraceTo(10004, DirectionType.Back, 6f, false);
            TryStartTraceTo(10001, DirectionType.Forward, 6f, false);
        }

        public override async UniTask RegroupToLeaderAsync(Vector3? targetPosition, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await base.RegroupToLeaderAsync(targetPosition, cancellationToken);

            try
            {
                if (!TryStartTraceTo(10004, DirectionType.Back, 6f, true))
                    return;

                if (!TryStartTraceTo(10001, DirectionType.Forward, 6f, true))
                    return;

                await UniTask.WaitUntil(() => _completedCount >= _totalMoveCount, cancellationToken: cancellationToken);
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