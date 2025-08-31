using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using Common;
using Creature;
using Creature.Action;
using Creature.Action.Weight;

namespace Battle
{
    public interface IWeightedActionRequester
    {
        WeightedActionParam GetWeightedActionParam(ICombatant attacker, IWeightedAction iWeightedAction);
        CancellationTokenSource CancellationTokenSource { get; }
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
        }

        void IWeightedActionController.Execute(ICombatant executer, IWeightedActionRequester iRequester)
        {
            ExecuteAsync(executer, iRequester).Forget();
        }

        private async UniTask ExecuteAsync(ICombatant executer, IWeightedActionRequester iRequester)
        {
            try
            {
                await UniTask.DelayFrame(30, cancellationToken: iRequester.CancellationTokenSource.Token);
                if (iRequester.CancellationTokenSource.IsCancellationRequested)
                {
                    EndAction(executer);
                    return;
                }

                var actionWeight = GetHighestPriorityActionWeight();
                var iWeightedAction = actionWeight?.Create();
                var param = iRequester?.GetWeightedActionParam(executer, iWeightedAction);

                iWeightedAction?.SetParam(param)?
                    .SetEndAction(EndAction)?
                    .SetIActor(executer)?
                    .Execute();
            }
            catch(OperationCanceledException)
            {
                EndAction(executer);
                // Debug.Log(executer.Id);
                // Debug.Log(exception);
            }
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
            if (iActor != null &&
               !iActor.IsActivate)
                return;

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
