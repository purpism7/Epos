using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Slot
{
    public class EmotionSlot : BaseSlot<EmotionSlot.Param>
    {
        public class Param : Common.Param
        {
            
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);
        }
    }
}

