using System.Collections.Generic;
using UnityEngine;

using Common;

namespace Creature.Action
{
    public interface IWeightedAciton
    {
        IWeightedAciton Initialize();

        void SetWeight(EWeightType eWeightType, int weight);
        bool CheckCondition { get; }

        int Weight { get; }
    }

    public class WeightedAction<T> : Act<T>, IWeightedAciton where T : Act<T>.ActParam
    {
        protected Dictionary<EWeightType, int> _eWeightTypeDic = new();

        protected virtual int Weight { get; }

        public IWeightedAciton Initialize()
        {
            return this;
        }

        int IWeightedAciton.Weight
        {
            get 
            {
                int weight = Weight;
                foreach(var value in _eWeightTypeDic.Values)
                {
                    weight += value;
                }

                return weight;
            }
        }

        void IWeightedAciton.SetWeight(EWeightType eWeightType, int weight)
        {
            _eWeightTypeDic[eWeightType] = 0;
            _eWeightTypeDic[eWeightType] += weight;
        }

        public virtual bool CheckCondition
        {
            get
            {
                return true;
            }
        }

        public override void Execute()
        {
            
        }
    }
}
