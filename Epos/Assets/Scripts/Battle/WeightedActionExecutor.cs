using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Creature;
using Creature.Action;

namespace Battle
{
    public interface IWeightedActionExecutor
    {
        void Initialize();

        void Execute(IActor iActor);
    }

    public class WeightedActionExecutor : IWeightedActionExecutor
    {
        private SortedSet<IWeightedAction> _iSortedWeightedActionSet = new(
            Comparer<IWeightedAction>.Create((action, compAction) =>
            {
                return compAction.Weight.CompareTo(action.Weight);
            }));

        private IWeightedAction _waitingIdle = null;

        void IWeightedActionExecutor.Initialize()
        {
            _iSortedWeightedActionSet?.Clear();
            _iSortedWeightedActionSet?.Add(CreateAction<ApproachAttack.Param>());
            _iSortedWeightedActionSet?.Add(CreateAction<CastSkill.Param>());

            _waitingIdle = CreateAction<WaitingIdle.Param>();
        }

        void IWeightedActionExecutor.Execute(IActor iActor)
        {
            Execute(iActor);
        }

        private void Execute(IActor iActor)
        {
            var iWeightedAction = GetHighestPriorityAction();
            iWeightedAction?.Execute(iActor);
        }

        private IWeightedAction CreateAction<T>(T t = null) where T : WeightedAction<T>.ActionParam, new()
        {
            var action = new WeightedAction<T>();
            
            action.SetParam(t);
            action.SetEndActAction(EndAction);
            action.Initialize();

            return action;
        }

        private void EndAction(IActor iActor)
        {
            Execute(iActor);
        }

        private IWeightedAction GetHighestPriorityAction()
        {
            foreach(var iWeightedAction in _iSortedWeightedActionSet)
            {
                if (iWeightedAction.CheckCondition())
                    return iWeightedAction;
            }

            return _waitingIdle;
        }
    }
}
