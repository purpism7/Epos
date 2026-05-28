using UnityEngine;
using Component = Common.Component;

using Common;
using Spine.Unity;

namespace UI
{
    public class CollectItem : Component<CollectItem.Param>
    {
        public class Param : Common.Param
        {
            
        }
        
        private const string IdleAnimationName = "Idle";
        
        [SerializeField] private SkeletonGraphic skeletonGraphic = null;

        public void PlayIdleAnimation()
        {
            if (skeletonGraphic == null)
                return;

            skeletonGraphic.PlayAnimation(IdleAnimationName, true, null, out _);
        }
    }
}
