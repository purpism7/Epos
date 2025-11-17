using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.View
{
    public class ShoutPanel : Common.Component<ShoutPanel.Param>
    {
        public class Param : Common.Param
        {
            // Add parameters here if needed in the future
        }

        public interface IListener
        {
            
        }

        [SerializeField] private Animator animator = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);
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
    }
}

