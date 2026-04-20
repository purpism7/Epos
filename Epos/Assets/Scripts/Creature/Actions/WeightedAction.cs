using System.Collections.Generic;
using System.Threading;
using Common;

namespace Creature.Actions
{
    public interface IWeightedAction
    {
        IWeightedAction SetParam<T>(T param);
        IWeightedAction SetEndAction(System.Action<IActor> endAction);
        IWeightedAction SetIActor(IActor iActor);

        void Execute();
    }

    public interface IWeightedActionInitializer
    {
        IWeightedAction Initialize();
    }

    public class WeightedActionParam : ActParam
    {
        public CancellationTokenSource CancellationTokenSource = null;
    }

    public abstract class WeightedAction<T> : Act<T>, IWeightedActionInitializer, IWeightedAction where T : ActParam
    {
        protected Dictionary<EWeightType, int> _eWeightTypeDic = new();

        protected virtual int Weight { get; }


        IWeightedAction IWeightedActionInitializer.Initialize()
        {
            return this;
        }

        IWeightedAction IWeightedAction.SetParam<V>(V param)
        {
            _param = param as T;
            return this;
        }

        IWeightedAction IWeightedAction.SetEndAction(System.Action<IActor> endAction)
        {
            _endAction = endAction;
            return this;
        }

        IWeightedAction IWeightedAction.SetIActor(IActor iActor)
        {
            SetIActor(iActor);
            //Execute();
            return this;
        }
    }
}
