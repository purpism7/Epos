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
                        .Timeout(TimeSpan.FromSeconds(5f));
            }
            catch (OperationCanceledException)
            {
                // 취소 시 깔끔하게 종료
            }
            catch (TimeoutException)
            {
                // ✨ 여기에 타임아웃 예외 처리를 추가합니다!
                UnityEngine.Debug.LogWarning("5초 타임아웃 발생: 유닛들이 시간 내에 모두 모이지 못했습니다.");

                // 필요하다면 타임아웃 시 강제로 유닛을 리더 옆으로 순간이동시키는 등의 후처리를 할 수 있습니다.
            }
            finally
            {
                CleanupTraceCallbacks();

                UnityEngine.Debug.Log("모두 집결 완료!");
            }
        }
    }
}

