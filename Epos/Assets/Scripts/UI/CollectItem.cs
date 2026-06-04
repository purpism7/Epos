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
        
        private const string MoveAnimationName = "Move";
        
        [SerializeField] private SkeletonGraphic skeletonGraphic = null;

        public void PlayMoveAnimation()
        {
            if (skeletonGraphic == null)
                return;

            skeletonGraphic.PlayAnimation(MoveAnimationName, true, null, out _);
        }
    }
}
