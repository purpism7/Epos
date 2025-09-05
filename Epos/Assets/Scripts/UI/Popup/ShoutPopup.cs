using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class ShoutPopup : BasePopup<ShoutPopup.Param>
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private Button cancelBtn = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            InitializeButton();

            return UniTask.CompletedTask;
        }

        public override void Activate()
        {
            base.Activate();

            Time.timeScale = 0;
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
