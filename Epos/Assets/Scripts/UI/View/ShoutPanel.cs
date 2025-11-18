using Cysharp.Threading.Tasks;
using UI.Slot;
using UnityEngine;

namespace UI.View
{
    public class ShoutPanel : Common.Component<ShoutPanel.Param>, ShoutSlot.IListener
    {
        public class Param : Common.Param
        {
            // Add parameters here if needed in the future
        }

        public interface IListener
        {
            
        }

        [SerializeField] private Animator animator = null;
        
        private ShoutSlot[] _shoutSlots = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);

            _shoutSlots = GetComponentsInChildren<ShoutSlot>();
            foreach (var shoutSlot in _shoutSlots)
            {
                shoutSlot?.InitializeAsync(new ShoutSlot.Param().WithListener(this));
            }
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

            animator?.SetBool("OnOff", false);
        }

        public override void Deactivate()
        {
            //base.Deactivate();

            animator?.SetBool("OnOff", true);
        }
        
        #region ShoutSlot.IListener

        void ShoutSlot.IListener.OnClick()
        {
            Deactivate();   
        }
        #endregion
    }
}

