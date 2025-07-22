using Creature;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField]
        private Slider hpSlider = null;

        public override void Initialize(Data data)
        {
            base.Initialize(data);
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            if(hpSlider != null &&
               data?.ICombatant != null)
            {
                hpSlider.maxValue = data.ICombatant.IStat.Get(Stat.EType.MaxHp);
            }
        }

        private void UpdateHp()
        {
            if (hpSlider == null)
                return;

            if (_data?.ICombatant == null)
                return;

            hpSlider.value = _data.ICombatant.IStat.Get(Stat.EType.Hp);
        }

        private void LateUpdate()
        {
            //Debug.Log(?.IStat?.Get(Stat.EType.Hp));
            if(_data.ICombatant.IsActivate)
            {
                ChainLateUpdate();
                UpdateHp();
            }
            else
                Deactivate();
        }
    }
}

