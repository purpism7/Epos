using UnityEngine;

namespace UI.Parts
{
    public class EmotionPart : PartWorld<EmotionPart.Param>
    {
        public class Param : PartParam
        {
        }
        
        private void LateUpdate()
        {
            ChainLateUpdate();
        }
    }
}

