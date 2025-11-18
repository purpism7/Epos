using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using TMPro.Examples;
using TMPro;
using UnityEngine.UI;

namespace UI.Slot
{
    public class ShoutSlot : BaseSlot<ShoutSlot.Param>
    {
        public class Param : Common.Param
        {
            public IListener Listener { get; private set; } = null;
            public EmotionType EmotionType { get; private set; } = EmotionType.None;

            public Param WithListener(IListener listener)
            {
                Listener = listener;
                return this;
            }
            
            public Param WithEmotionType(EmotionType emotionType)
            {
                EmotionType = emotionType;
                return this;
            }
        }

        public interface IListener
        {
            void OnClick();
        }

        [SerializeField] private TextMeshProUGUI emotionTMP = null;
        [SerializeField] private EmotionType emotionType = EmotionType.None;
        [SerializeField] private Button btn = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            btn?.onClick.RemoveAllListeners();
            btn?.onClick?.AddListener(OnClick);
            
            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            emotionTMP?.SetText(param?.EmotionType.ToString());

            return UniTask.CompletedTask;
        }

        private void OnClick()
        {
            _param?.Listener?.OnClick();
        }
    }
}
