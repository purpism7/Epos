using System;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.UI;

using VContainer;
using Cysharp.Threading.Tasks;

using UI.Slot;
using Creator;

namespace UI.Popup
{
    public class ShoutPopup : BasePopup<ShoutPopup.Param>
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private RectTransform shoutSlotRootRectTm = null;
        [SerializeField] private Button cancelBtn = null;

        [Inject] private UIFactory _uiFactory = null;

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

            Time.timeScale = 0.2f;
        }

        private void InitializeShoutSlotList()
        {
            _shoutSlotList?.Clear();

            var uiCreator = _uiFactory.Create<ShoutSlot, ShoutSlot.Param>();
            var shoutSlotParam = new ShoutSlot.Param();

            foreach (EEmotionType eEmotionType in Enum.GetValues(typeof(EEmotionType)))
            {
                if (eEmotionType == EEmotionType.None)
                    continue;

                shoutSlotParam?.WithEEmotionType(eEmotionType);

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
