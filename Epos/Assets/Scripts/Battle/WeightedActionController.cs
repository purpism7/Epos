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
using Random = UnityEngine.Random;

namespace Battle
{
    public interface IWeightedActionRequester
    {
        WeightedActionParam GetWeightedActionParam(ICombatant attacker, ETeam eTeam, IWeightedAction iWeightedAction);
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
            void End(IActor iActor);
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
                var cancellationTokenSource = iRequester.CancellationTokenSource;
                if (cancellationTokenSource != null)
                {
                    await UniTask.DelayFrame(30, cancellationToken: cancellationTokenSource.Token);
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        EndAction(executer.IActor);
                        return;
                    }
                }
                
                var actionWeight = GetHighestPriorityActionWeight();
                var iWeightedAction = actionWeight?.Create();
                var param = iRequester.GetWeightedActionParam(executer, executer.ETeam, iWeightedAction);

                iWeightedAction?.SetParam(param)?
                    .SetEndAction(EndAction)?
                    .SetIActor(executer.IActor)?
                    .Execute();
            }
            catch(OperationCanceledException)
            {
                EndAction(executer.IActor);
            }
        }
        
        private void EndAction(IActor iActor)
        {
            if (iActor != null &&
               !iActor.IsActivate)
                return;

            _iListener?.End(iActor);
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
