using Creature;
using UnityEngine;
using UnityEngine.UI;

namespace  UI.Parts
{
    public class HpProgress : PartWorld<HpProgress.Param>
    {
        public class Param : PartWorld<Param>.PartParam
        {
            public ICombatant ICombatant { get; private set; } = null;

            public Param WithCombatant(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
                return this;
            }
        }

        [SerializeField]
        private Slider hpSlider = null;

        public override void Initialize(Param param)
        {
            base.Initialize(param);
        }

        public override void Activate(Param param)
        {
            base.Activate(param);

            if(hpSlider != null &&
               param?.ICombatant != null)
            {
                hpSlider.maxValue = param.ICombatant.IStat.Get(Stat.EType.MaxHp);
            }
        }

        private void UpdateHp()
        {
            if (hpSlider == null)
                return;

            if (_param?.ICombatant == null)
                return;

            hpSlider.value = _param.ICombatant.IStat.Get(Stat.EType.Hp);
        }

        private void LateUpdate()
        {
            //Debug.Log(?.IStat?.Get(Stat.EType.Hp));
            if(_param.ICombatant.IsActivate)
            {
                ChainLateUpdate();
                UpdateHp();
            }
            else
                Deactivate();
        }
    }
}

