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
            public EEmotionType EEmotionType { get; private set; } = EEmotionType.None;

            public Param WithEEmotionType(EEmotionType eEmotionType)
            {
                EEmotionType = eEmotionType;
                return this;
            }
        }

        [SerializeField] private TextMeshProUGUI emotionTMP = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            emotionTMP?.SetText(param?.EEmotionType.ToString());

            return UniTask.CompletedTask;
        }
    }
}
