using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class ActData
    {
        public IActor IActor = null;
    }
    
    public interface IAct
    {
        void Finish();
        void ChainUpdate();
    }
    
    public abstract class Act<T> : IAct where T : ActData
    {
         protected T _data = null;
        
        #region IAct

        public virtual void Execute(T data)
        {
            _data = data;
        }

        public virtual void Finish()
        {
            
        }

        public virtual void ChainUpdate()
        {
            
        }
        #endregion

        protected void SetAnimation(string animationName, bool loop)
        {
            var iActor = _data?.IActor;
            if (iActor == null)
                return;
            
            var animationState = iActor.SkeletonAnimation?.AnimationState;
            if (animationState == null)
                return;
            
            var trackEntry = animationState.SetAnimation(0, animationName, loop);
            if (trackEntry == null)
                return;
        }
    }
}

