using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using UI.Slot;

namespace UI.View
{
    public class ShoutPanel : Common.Component<ShoutPanel.Param>, ShoutSlot.IListener
    {
        public class Param : Common.Param
        {
            public IListener Listener { get; private set; } = null;

            public Param(IListener listener)
            {
                Listener = listener;
            }
        }

        public interface IListener
        {
            void OnSelectShout(EmotionType emotionType);
        }

        [SerializeField] private Animator animator = null;
        
        private ShoutSlot[] _shoutSlots = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);

            _shoutSlots = GetComponentsInChildren<ShoutSlot>(true);
            foreach (var shoutSlot in _shoutSlots)
            {
                shoutSlot?.InitializeAsync(new ShoutSlot.Param().WithListener(this));
            }
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

           
        }

        public override void Activate()
        {
            base.Activate();
            
            animator?.SetBool("OnOff", false);
        }

        public override void Deactivate()
        {
            //base.Deactivate();

            animator?.SetBool("OnOff", true);
        }
        
        #region ShoutSlot.IListener
        void ShoutSlot.IListener.OnClick(EmotionType emotionType)
        {
            Deactivate();   
            
            _param?.Listener?.OnSelectShout(emotionType);
        }
        #endregion
    }
}

