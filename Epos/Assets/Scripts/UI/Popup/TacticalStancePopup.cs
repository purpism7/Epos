using System;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;

using VContainer;
using Cysharp.Threading.Tasks;

using UI.Slot;
using Creator;
using GameSystem;

namespace UI.Popup
{
    public class TacticalStancePopup : BasePopup<TacticalStancePopup.Param>
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private RectTransform shoutSlotRootRectTm = null;
        [SerializeField] private Button cancelBtn = null;

        [Inject] private ITimeScaleManager _iTimeScaleManager = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            InitializeButton();

            return UniTask.CompletedTask;
        }

        public override void Activate()
        {
            base.Activate();

            _iTimeScaleManager?.Set(0.2f);
        }

        public override void Deactivate()
        {
            base.Deactivate();

            _iTimeScaleManager?.Set(1f);
        }

        private void InitializeButton()
        {
            cancelBtn?.onClick?.AddListener(() => OnClickCancel());
        }

        private void OnClickCancel()
        {
            Deactivate();
        }
    }
}
