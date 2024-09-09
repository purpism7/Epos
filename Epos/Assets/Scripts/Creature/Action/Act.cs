using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface IAct
    {
        void Execute(IActor iActor);
    }
    
    public abstract class Act : IAct
    {
        public abstract void Execute(IActor iActor);

        protected void SetAnimation(IActor iActor, string animationName, bool loop)
        {
            var animationState = iActor?.SkeletonAnimation?.AnimationState;
            if (animationState == null)
                return;
            
            var trackEntry = animationState.SetAnimation(0, animationName, loop);
            if (trackEntry == null)
                return;
        }
    }
}

