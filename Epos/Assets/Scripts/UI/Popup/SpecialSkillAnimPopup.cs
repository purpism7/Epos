using Common;
using Creator;
using Cysharp.Threading.Tasks;
using GameSystem;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UI.Slot;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Popup
{
    public class SpecialSkillAnimPopup : BasePopup<SpecialSkillAnimPopup.Param>
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private SkeletonGraphic skeletonGraphic = null;

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

            skeletonGraphic?.PlayAnimation("Skill_01", false, 
                (trackEntry) =>
                {
                    Deactivate();

                    _iTimeScaleManager?.Resume();
                }, out float duration);
        }

        public override void Deactivate()
        {
            base.Deactivate();

        }
    }
}
