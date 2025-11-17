using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using TMPro.Examples;
using TMPro;

namespace UI.Slot
{
    public class ShoutSlot : BaseSlot<ShoutSlot.Param>
    {
        public class Param : Common.Param
        {
            public EmotionType EmotionType { get; private set; } = EmotionType.None;

            public Param WithEmotionType(EmotionType emotionType)
            {
                EmotionType = emotionType;
                return this;
            }
        }

        [SerializeField] private TextMeshProUGUI emotionTMP = null;
        [SerializeField] private EmotionType emotionType = EmotionType.None;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            emotionTMP?.SetText(param?.EmotionType.ToString());

            return UniTask.CompletedTask;
        }
    }
}
