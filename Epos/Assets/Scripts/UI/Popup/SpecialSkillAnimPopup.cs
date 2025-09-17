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


        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        public override void Activate()
        {
            base.Activate();

            _iTimeScaleManager?.Pause();

            float duration = 0;
            effectSkeletonGraphic?.PlayAnimation("Skill_Effect_Ch_01", false, null, out duration);
            skeletonGraphic?.PlayAnimation("Skill_Ch_E_01", false, 
                (trackEntry) =>
                {
                    Deactivate();

                    _iTimeScaleManager?.Resume();
                }, out duration);
        }

        public override void Deactivate()
        {
            base.Deactivate();

        }
    }
}
