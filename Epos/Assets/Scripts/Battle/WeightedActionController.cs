using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Common;
using Creature;
using Creature.Action;
using Creature.Action.Weight;
using Cysharp.Threading.Tasks;
using System;

namespace Battle
{
    public interface IWeightedActionRequester
    {
        WeightedActionParam GetWeightedActionParam(ICombatant attacker, IWeightedAction iWeightedAction);
    }

    public interface IWeightedActionController
    {
        void Initialize(WeightedActionController.IListener iListener);

        void Execute(ICombatant executer, IWeightedActionRequester iRequester);
    }

    public class WeightedActionController : IWeightedActionController
    {
        public interface IListener
        {
            void End(ICombatant iCombatant);
        }

        private IListener _iListener = null;

        private SortedSet<ActionWeight> _iSortedActionWeightSet = new(
            Comparer<ActionWeight>.Create((action, compAction) =>
            {
                return compAction.Weight.CompareTo(action.Weight);
            }));

        void IWeightedActionController.Initialize(IListener iListener)
        {
            _iListener = iListener;

            _iSortedActionWeightSet?.Clear();
            _iSortedActionWeightSet?.Add(new Creature.Action.Weight.ApproachAttack());
            _iSortedActionWeightSet?.Add(new Creature.Action.Weight.CastSkill());
            _iSortedActionWeightSet?.Add(new Creature.Action.Weight.WaitingIdle());
            //_iSortedActionWeightSet?.Add(CreateActionWeight<CastSkill>());
            //_iSortedActionWeightSet?.Add(CreateActionWeight<WaitingIdle>());

            //_waitingIdle = CreateActionWeight<WaitingIdle>();
        }

        void IWeightedActionController.Execute(ICombatant executer, IWeightedActionRequester iRequester)
        {
            ExecuteAsync(executer, iRequester).Forget();
        }

        private async UniTask ExecuteAsync(ICombatant executer, IWeightedActionRequester iRequester)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            await UniTask.Yield();
            //await UniTask.Delay(TimeSpan.FromSeconds(1f),false);

            var actionWeight = GetHighestPriorityActionWeight();
            var iWeightedAction = actionWeight?.Create();
            var param = iRequester?.GetWeightedActionParam(executer, iWeightedAction);

            iWeightedAction?.SetParam(param)?
                .SetEndAction(EndAction)?
                .SetIActor(executer)?
                .Execute();
        }

        //private IWeightedAction CreateAction<T, V>() where T : new() where V : WeightedActionParam
        //{
        //    var action = new T() as WeightedAction<V>;
        //    //action.SetParam(tParam);
        //    action.SetEndActAction(EndAction);
        //    action.Initialize();

        //    return action;
        //}

        private void EndAction(IActor iActor)
        {
            _iListener?.End(iActor as ICombatant);
        }

        private ActionWeight GetHighestPriorityActionWeight()
        {
            foreach(var actionWeight in _iSortedActionWeightSet)
            {
                if (actionWeight.CheckCondition())
                    return actionWeight;
            }

            return null;
        }
    }
}
