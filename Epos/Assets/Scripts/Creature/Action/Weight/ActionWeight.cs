using Common;
using Creature.Action;
using System.Collections.Generic;

namespace Creature.Action.Weight
{
    public abstract class ActionWeight
    {
        protected Dictionary<EWeightType, int> _eWeightTypeDic = new();

        protected abstract int DefaultWeight { get; }

        public abstract IWeightedAction Create();

        public int Weight
        {
            get
            {
                int weight = DefaultWeight;
                foreach (var value in _eWeightTypeDic.Values)
                {
                    weight += value;
                }

                return weight;
            }
        }

        public void SetWeight(EWeightType eWeightType, int weight)
        {
            if (eWeightType == EWeightType.None)
                return;

            _eWeightTypeDic[eWeightType] = 0;
            _eWeightTypeDic[eWeightType] += weight;
        }

        public virtual bool CheckCondition()
        {
            return true;
        }
    }
}
