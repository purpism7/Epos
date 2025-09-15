using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using GameSystem;
using Creator;

namespace UI.Popup
{ 
    public abstract class BasePopup<T> : Common.Component<T> where T : Common.Param
    {
        [Inject] protected UIManager _uiManager = null;
        [Inject] protected UIFactory _uiFactory = null;

        public override void Deactivate()
        {
            base.Deactivate();

            _uiManager?.SetCurrPopup(null);

            Return();
        }
    }
}
