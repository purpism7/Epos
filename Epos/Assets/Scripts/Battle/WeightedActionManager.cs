using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Creature.Action;

namespace Battle
{
    public interface IWeightedActionManager
    {
        void Initialize();
    }

    public class WeightedActionManager : IWeightedActionManager
    {
        private SortedSet<IWeightedAciton> _iSortedWeightedActionSet = new(
            Comparer<IWeightedAciton>.Create((action, compAction) =>
            {
                return action.Weight.CompareTo(compAction.Weight);
            }));

        private IWeightedAciton _waitingIdle = null;

        void IWeightedActionManager.Initialize()
        {
            _iSortedWeightedActionSet?.Clear();
            _iSortedWeightedActionSet?.Add(CreateAction<ApproachAttack.Param>());
            _iSortedWeightedActionSet?.Add(CreateAction<CastSkill.Param>());

            _waitingIdle = CreateAction<WaitingIdle.Param>();
        }

        private IWeightedAciton CreateAction<T>() where T : WeightedAction<T>.ActParam, new()
        {
            var action = new WeightedAction<T>();
            action?.Initialize();

            return action;
        }

        private IWeightedAciton GetHighestPriorityAction()
        {
            foreach(var iWeightedAction in _iSortedWeightedActionSet)
            {
                if (iWeightedAction.CheckCondition)
                    return iWeightedAction;
            }

            return _waitingIdle;
        }
    }
}
