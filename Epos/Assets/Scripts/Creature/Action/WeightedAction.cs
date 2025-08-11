using System.Collections.Generic;
using UnityEngine;

using Common;
using Unity.VisualScripting;

namespace Creature.Action
{
    public interface IWeightedAction
    {
        void SetWeight(EWeightType eWeightType, int weight);
        bool CheckCondition { get; }

        int Weight { get; }
    }

    public interface IWeightedActionInitializer
    {
        IWeightedAction Initialize();
    }

    public class WeightedAction<T> : Act<T>, IWeightedActionInitializer, IWeightedAction where T : Act<T>.ActParam
    {
        public class ActionParam : ActParam
        {

        }

        protected Dictionary<EWeightType, int> _eWeightTypeDic = new();

        protected virtual int Weight { get; }

        public IWeightedAction Initialize()
        {
            return this;
        }

        int IWeightedAction.Weight
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

        void IWeightedAction.SetWeight(EWeightType eWeightType, int weight)
        {
            if (eWeightType == EWeightType.None)
                return;

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
