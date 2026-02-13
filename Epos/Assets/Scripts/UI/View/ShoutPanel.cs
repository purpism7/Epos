using System;
using UnityEngine;

using Cysharp.Threading.Tasks;
using TMPro;

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
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField] private string[] shoutDescriptions = null;

        private ShoutSlot[] _shoutSlots = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);

            _shoutSlots = GetComponentsInChildren<ShoutSlot>(true);
            for(int i = 0; i < _shoutSlots?.Length; ++i)
            {
                var shoutSlot = _shoutSlots[i];
                var description = (i < shoutDescriptions.Length) ? shoutDescriptions[i] : string.Empty;
                var emotionType = (EmotionType)(i + 1); // Assuming EmotionType enum starts from 1 for valid types
                
                await shoutSlot.InitializeAsync(new ShoutSlot.Param()
                    .WithListener(this)
                    .WithEmotionType(emotionType)
                    .WithDescription(description));
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

            // animator?.SetBool("OnOff", true);
        }

        private async UniTask PlayEndAnimationAsync(EmotionType emotionType)
        {
            if (animator == null)
                return;
            
            animator?.SetBool("OnOff", true);

            await UniTask.NextFrame();
            await UniTask.WaitUntil(() =>
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("On"))
                {
                    Debug.Log("On");

                    // return true;
                }

                if (stateInfo.IsName("Off"))
                {
                    Debug.Log("Off");
                    return true;
                    // return true;
                }
                
                return false;
            });

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            
            _param?.Listener?.OnSelectShout(emotionType);
        }
        
        #region ShoutSlot.IListener
        void ShoutSlot.IListener.OnClick(EmotionType emotionType, string description)
        {
            descriptionText?.SetText(description);

            Deactivate();   
            PlayEndAnimationAsync(emotionType).Forget();
        }
        #endregion
    }
}

