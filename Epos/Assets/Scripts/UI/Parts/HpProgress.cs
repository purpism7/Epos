using Creature;
using UnityEngine;

namespace  UI.Parts
{
    public class HpProgress : PartWorld<HpProgress.Data>
    {
        public class Data : PartWorld<Data>.Data
        {
            public ICombatant ICombatant { get; private set; } = null;

            public Data WithCombatant(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
                return this;
            }
        }

        public override void Initialize(Data data)
        {
            base.Initialize(data);
        }

        private void LateUpdate()
        {
            //if(_data?.TargetTm)
            //Debug.Log(_data?.ICombatant?.IStat?.Get(Stat.EType.Hp));
            ChainLateUpdate();
        }
    }
}

