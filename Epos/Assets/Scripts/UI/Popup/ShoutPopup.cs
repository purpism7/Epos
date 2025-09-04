using UnityEngine;

namespace UI.Popup
{
    public class ShoutPopup : BasePopup<ShoutPopup.Param>
    {
        public class Param : Common.Param
        {
            
        }

        public override void Activate()
        {
            base.Activate();

            Time.timeScale = 0;
        }
    }
}
