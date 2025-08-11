using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Creature.Action;

namespace Battle
{
    public interface IWeightedActionExecutor
    {
        void Initialize();

        void Execute(IActController iActCtr);
    }

    public class WeightedActionExecutor : IWeightedActionExecutor
    {
        private SortedSet<IWeightedAction> _iSortedWeightedActionSet = new(
            Comparer<IWeightedAction>.Create((action, compAction) =>
            {
                return action.Weight.CompareTo(compAction.Weight);
            }));

        private IWeightedAction _waitingIdle = null;

        void IWeightedActionExecutor.Initialize()
        {
            _iSortedWeightedActionSet?.Clear();
            _iSortedWeightedActionSet?.Add(CreateAction<ApproachAttack.Param>());
            _iSortedWeightedActionSet?.Add(CreateAction<CastSkill.Param>());

            _waitingIdle = CreateAction<WaitingIdle.Param>();
        }

        void IWeightedActionExecutor.Execute(IActController iActCtr)
        {


            //iActCtr.CastingSkill

            //iActCtr
        }

        private IWeightedAction CreateAction<T>(T t = null) where T : WeightedAction<T>.ActionParam, new()
        {
            var action = new WeightedAction<T>();
            action?.SetParam(t);
            action?.SetEndActAction(FinsishAction);
            action?.Initialize();

            return action;
        }

        private void FinsishAction()
        {

        }
       

        //private IWeightedAciton GetHighestPriorityAction()
        //{
        //    foreach(var iWeightedAction in _iSortedWeightedActionSet)
        //    {
        //        if (iWeightedAction.CheckCondition)
        //            return iWeightedAction;
        //    }

        //    return _waitingIdle;
        //}
    }
}
