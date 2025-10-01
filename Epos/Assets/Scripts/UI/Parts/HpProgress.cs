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
        UniTask ActivateAsync(HpProgress.Param param);
        void Deactivate();
        //void ChainLateUpdate();
        
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

        [SerializeField] private Slider previewMpSlider = null;
        [SerializeField] private Slider mpSlider = null;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override UniTask ActivateAsync(Param param)
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

        private void LateUpdate()
        {
            ChainLateUpdate();
        }

        void IHpProgress.UpdateHpProgress()
        {
            UpdateProgress(Stat.EType.Hp, hpSlider, previewHpSlider);
        }

        private void SetHpProgress()
        {
            var iActor = _param?.ICombatant?.IActor;
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

        private void SetMpProgress()
        {
            var iActor = _param?.ICombatant?.IActor;
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
            var iActor = _param?.ICombatant?.IActor;
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

            if (eventData.CharacterId != _param.ICombatant.IActor.Id)
                return;

            UpdateProgress(Stat.EType.Hp, hpSlider, previewHpSlider);
            UpdateProgress(Stat.EType.Mp, mpSlider);
        }
    }
}

