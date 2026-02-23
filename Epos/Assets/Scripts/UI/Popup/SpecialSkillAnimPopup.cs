using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

using Spine.Unity;
using Cysharp.Threading.Tasks;
using VContainer;

using GameSystem;
using Common;
using Creator;
using UI.Slot;

namespace UI.Popup
{
    public class SpecialSkillAnimPopup : BasePopup<SpecialSkillAnimPopup.Param>
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private SkeletonGraphic skeletonGraphic = null;
        [SerializeField] private SkeletonGraphic effectSkeletonGraphic = null;

        [Inject] private ITimeScaleManager _iTimeScaleManager = null;

        private int _playSequenceId = 0;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        public override void Activate()
        {
            base.Activate();

            _playSequenceId++;
            _iTimeScaleManager?.Pause();

            PlaySequenceAsync(_playSequenceId).Forget();
        }

        public override void Deactivate()
        {
            _playSequenceId++;
            _iTimeScaleManager?.Resume();

            base.Deactivate();
        }

        private async UniTaskVoid PlaySequenceAsync(int sequenceId)
        {
            float effectDuration = 0f;
            float mainDuration = 0f;

            effectSkeletonGraphic?.PlayAnimation("Skill_Eff_Ch_01", false, null, out effectDuration);
            skeletonGraphic?.PlayAnimation("Skill_Kinght_01", false, null, out mainDuration);

            float waitDuration = Mathf.Max(effectDuration, mainDuration);
            if (waitDuration <= 0f)
                waitDuration = 0.1f;

            await UniTask.Delay(TimeSpan.FromSeconds(waitDuration), DelayType.UnscaledDeltaTime);

            if (!IsActivate || sequenceId != _playSequenceId)
                return;

            Deactivate();
        }
    }
}
