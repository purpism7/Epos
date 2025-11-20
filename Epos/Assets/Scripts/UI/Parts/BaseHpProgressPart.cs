using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameSystem.Event;
using System;

using Creature;

namespace  UI.Parts
{
    public interface IHpProgress
    {
        // UniTask ActivateAsync(HpProgress.Param param);
        void Deactivate();
        //void ChainLateUpdate();
        
        void UpdateHpProgress();
    }
    
    public abstract class BaseHpProgressPart<T> : PartWorld<T>, IHpProgress where T : PartWorld<T>.PartParam
    {
        public class Param : PartWorld<T>.PartParam
        {
            public ICombatant ICombatant { get; private set; } = null;

            public Param WithCombatant(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
                return this;
            }
        }

        [SerializeField] protected Slider previewHpSlider = null;
        [SerializeField] protected Slider hpSlider = null;

        [SerializeField] protected Slider previewMpSlider = null;
        [SerializeField] protected Slider mpSlider = null;

        protected ICombatant _ICombatant = null;
        
        private void LateUpdate()
        {
            ChainLateUpdate();
        }
        
        public override void Initialize()
        {
            base.Initialize();
        }

        public override UniTask ActivateAsync(T param)
        {
            base.ActivateAsync(param);

            SetHpProgress();
            SetMpProgress();

            GameSystem.Event.EventHandler.Add<StatChangedEventData>(OnStatChanged);
            
            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            GameSystem.Event.EventHandler.Remove<StatChangedEventData>(OnStatChanged);

            Return();
        }

        public virtual void UpdateHpProgress()
        {
            UpdateProgress(Stat.EType.Hp, hpSlider, previewHpSlider);
        }
        
        protected void SetHpProgress()
        {
            var iActor = _ICombatant?.IActor;
            if (iActor == null)
                return;

            var maxHp = iActor.IStat.Get(Stat.EType.MaxHp);

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
        }

        protected void SetMpProgress()
        {
            var iActor = _ICombatant?.IActor;
            if (iActor == null)
                return;

            var maxMp = iActor.IStat.Get(Stat.EType.MaxMp);

            if (previewMpSlider != null)
            {
                previewMpSlider.maxValue = maxMp;
                previewMpSlider.value = maxMp;
            }

            if (mpSlider != null)
            {
                mpSlider.maxValue = maxMp;
                mpSlider.value = maxMp;
            }
        }
        
        private void UpdateProgress(Stat.EType eType, Slider slider, Slider previewSlider = null)
        {
            var iActor = _ICombatant?.IActor;
            if (iActor == null)
                return;

            if (slider != null)
            {
                var value = iActor.IStat.Get(eType);

                slider.DOValue(value, 0.1f)
                    .OnComplete(() =>
                    {
                        if (previewSlider != null)
                            previewSlider.DOValue(value, 0.3f);

                        if (eType == Stat.EType.Hp &&
                            !iActor.IsAlive)
                            Deactivate();
                    });
            }  
        }

        private void OnStatChanged(StatChangedEventData eventData)
        {
            if (eventData == null ||
                _param == null)
                return;

            if (eventData.CharacterId != _ICombatant.IActor.Id)
                return;

            UpdateProgress(Stat.EType.Hp, hpSlider, previewHpSlider);
            UpdateProgress(Stat.EType.Mp, mpSlider);
        }
    }
}

