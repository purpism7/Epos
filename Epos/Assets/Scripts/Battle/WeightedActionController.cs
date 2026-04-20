using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using Common;
using Creature;
using Creature.Actions;
using Creature.Actions.Weight;

namespace Battle
{
    public interface IWeightedActionRequester
    {
        WeightedActionParam GetWeightedActionParam(ICombatant attacker, TeamType teamType, IWeightedAction iWeightedAction);
        //CancellationTokenSource CancellationTokenSource { get; }
    }

    public interface IWeightedActionController
    {
        void Initialize(WeightedActionController.IListener iListener);

        void Execute(ICombatant executer, IWeightedActionRequester iRequester, bool isFirst = false);
    }

    public class WeightedActionController : IWeightedActionController
    {
        public interface IListener
        {
            void End(IActor iActor);
        }

        private IListener _iListener = null;

        private readonly SortedSet<ActionWeight> _sortedActionWeightSet = new(
            Comparer<ActionWeight>.Create((action, compAction) =>
            {
                return compAction.Weight.CompareTo(action.Weight);
            }));

        void IWeightedActionController.Initialize(IListener iListener)
        {
            _iListener = iListener;

            _sortedActionWeightSet?.Clear();
            _sortedActionWeightSet?.Add(new Creature.Actions.Weight.ApproachAttack());
            _sortedActionWeightSet?.Add(new Creature.Actions.Weight.CastSkill());
            _sortedActionWeightSet?.Add(new Creature.Actions.Weight.WaitingIdle());
        }

        void IWeightedActionController.Execute(ICombatant executor, IWeightedActionRequester iRequester, bool isFirst)
        {
            ExecuteAsync(executor, iRequester, isFirst).Forget();
        }

        private async UniTask ExecuteAsync(ICombatant executor, IWeightedActionRequester requester, bool isFirst)
        {
            try
            {
                var actionWeight = GetHighestPriorityActionWeight();
                var weightedAction = actionWeight?.Create();
                
                // 액션이 유효하다면 실행
                if (weightedAction != null)
                {
                    var param = requester.GetWeightedActionParam(executor, executor.TeamType, weightedAction);

                    var cancellationTokenSource = param.CancellationTokenSource;
                    if (cancellationTokenSource != null)
                    {
                        float seconds = 0;
                        if (executor.TeamType == TeamType.Enemy && isFirst)
                            seconds += UnityEngine.Random.Range(0, 1f);
  
                        await UniTask.Delay(Mathf.RoundToInt(seconds * 1000f), cancellationToken: cancellationTokenSource.Token);
                    }

                    weightedAction.SetParam(param)
                        .SetEndAction(EndAction)
                        .SetIActor(executor.Actor)
                        .Execute();
                }
                else
                {
                    // 중요: 수행할 액션이 없더라도 턴/행동을 종료 처리는 해야 함
                    // Debug.LogWarning($"[{executer.IActor?.Name}] No valid action weight found. Skipping turn.");
                    EndAction(executor.Actor);
                }
            }
            catch (OperationCanceledException)
            {
                // 취소 발생 시 종료 처리
                EndAction(executor.Actor);
            }
            catch (Exception e)
            {
                // 예상치 못한 에러 발생 시에도 게임이 멈추지 않도록 종료 처리 권장
                Debug.LogError(e);
                EndAction(executor.Actor);
            }
        }
        
        private void EndAction(IActor actor)
        {
            if (actor != null &&
               !actor.IsActivate)
                return;

            _iListener?.End(actor);
        }

        private ActionWeight GetHighestPriorityActionWeight()
        {
            foreach(var actionWeight in _sortedActionWeightSet)
            {
                if (actionWeight.CheckCondition())
                    return actionWeight;
            }

            return null;
        }
    }
}
