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
    public class ShoutPopup : BasePopup<ShoutPopup.Param>
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private RectTransform shoutSlotRootRectTm = null;
        [SerializeField] private Button cancelBtn = null;


        [Inject] private ITimeScaleManager _iTimeScaleManager = null;
        [Inject] private IObjectResolver _iResolver = null;

        private List<ShoutSlot> _shoutSlotList = new();

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            InitializeShoutSlotList();
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

        private void InitializeShoutSlotList()
        {
            _shoutSlotList?.Clear();

            var uiCreator = _uiFactory.Create<ShoutSlot, ShoutSlot.Param>(_iResolver);
            var shoutSlotParam = new ShoutSlot.Param();

            foreach (EmotionType emotionType in Enum.GetValues(typeof(EmotionType)))
            {
                if (emotionType == EmotionType.None)
                    continue;

                shoutSlotParam.WithEmotionType(emotionType);

                var shoutSlot = uiCreator?
                    .SetParam(shoutSlotParam)?
                    .SetRoot(shoutSlotRootRectTm)?
                    .Create();
                shoutSlot?.ActivateAsync(shoutSlotParam);

                _shoutSlotList?.Add(shoutSlot);
            }
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
