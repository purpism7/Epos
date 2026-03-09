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
            public string Description { get; private set; } = string.Empty;

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

            public Param WithDescription(string description)
            {
                Description = description;
                return this;
            }
        }

        public interface IListener
        {
            void OnSelect(EmotionType emotionType);
            void OnConfirm(EmotionType emotionType, string description);
        }

        [SerializeField] private TextMeshProUGUI emotionTMP = null;
        [SerializeField] private EmotionType emotionType = EmotionType.None;
        [SerializeField] private Button btn = null;
        [SerializeField] private Button confirmBtn = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            btn?.onClick.RemoveAllListeners();
            btn?.onClick?.AddListener(OnClick);

            confirmBtn?.onClick?.RemoveAllListeners();
            confirmBtn?.onClick?.AddListener(OnConfirm);


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
            _param?.Listener?.OnSelect(emotionType);
        }

        private void OnConfirm()
        {
            _param?.Listener?.OnConfirm(emotionType, _param?.Description);
        }
    }
}
