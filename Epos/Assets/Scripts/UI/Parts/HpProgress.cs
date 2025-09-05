using Creature;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameSystem.Event;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace  UI.Parts
{
    public interface IHpProgress
    {
        void Activate(HpProgress.Param param);
        void Deactivate();
        void ChainLateUpdate();
        
        void UpdateHpProgress();
    }
    
    public class HpProgress : PartWorld<HpProgress.Param>, IHpProgress
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

        [SerializeField] private Slider previewHpSlider = null;
        [SerializeField] private Slider hpSlider = null;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Activate(Param param)
        {
            base.Activate(param);

            if(param?.ICombatant != null)
            {
                var maxHp = param.ICombatant.IStat.Get(Stat.EType.MaxHp);
                
                if (previewHpSlider != null)
                {
                    previewHpSlider.maxValue = maxHp;
                    previewHpSlider.value = maxHp;
                }

                if (hpSlider != null)
                {
                    hpSlider.maxValue = maxHp;
                    hpSlider.value = maxHp;
                }

                GameSystem.Event.EventHandler.Add<StatChangedEventData>(OnStatChanged);
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            GameSystem.Event.EventHandler.Remove<StatChangedEventData>(OnStatChanged);
        }
        
        void IHpProgress.UpdateHpProgress()
        {
            UpdateHpProgress();
        }

        private void UpdateHpProgress()
        {
            if (_param?.ICombatant == null)
                return;

            var hp = _param.ICombatant.IStat.Get(Stat.EType.Hp);

            if (hpSlider != null)
                hpSlider.DOValue(hp, 0.1f)
                    .OnComplete(() =>
                    {
                        if (previewHpSlider != null)
                            previewHpSlider.DOValue(hp, 0.3f);

                        if (hp <= 0)
                            Deactivate();
                    });
        }

        private void OnStatChanged(StatChangedEventData eventData)
        {
            if (eventData == null ||
                _param == null)
                return;

            if (eventData.CharacterId != _param.ICombatant.Id)
                return;

            UpdateHpProgress();
        }
    }
}

