using UnityEngine;
using Cysharp.Threading.Tasks;

using Common;

namespace UI
{
    public class PortraitEmotionEmoji : BaseSlot<PortraitEmotionEmoji.Param>
    {
        public class Param : Common.Param
        {
            
        }

        [SerializeField] private EmotionType emotionType = EmotionType.None;
        
        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);
        }
    }
}

