using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface IAct
    {
        void Execute();
        void ChainUpdate();
        void ChainFixedUpdate();
    }
    
    public abstract class Act<T> : IAct where T : Act<T>.BaseData
    {
        public class BaseData
        {
            public string AnimationKey { get; private set; } = string.Empty;
            
            public BaseData SetAnimationKey(string key)
            {
                AnimationKey = key;

                return this;
            }
        }
        
        protected T _data = null;
        protected IActor _iActor = null;
        protected System.Action _endAction = null;
        protected float _duration = 0;
        
        public virtual void Initialize(IActor iActor)
        {
            _iActor = iActor;
        }

        public void SetData(T data)
        {
            _data = data;
        }

        public void SetEndActAction(System.Action endAction)
        {
            _endAction = endAction;
        }
        
        #region IAct
        public abstract void Execute();
        
        public virtual void ChainUpdate()
        {
            
        }

        public virtual void ChainFixedUpdate()
        {
            
        }
        #endregion

        protected void SetAnimation(string animationName, bool loop)
        {
            if (_iActor == null)
                return;
            
            var animationState = _iActor.SkeletonAnimation?.AnimationState;
            if (animationState == null)
                return;
            
            var animation = _iActor.SkeletonAnimation.skeletonDataAsset?.GetSkeletonData(true)?.Animations?
                .Find(animation => animation.Name.Contains(animationName));
            if (animation == null)
                return;
            
            var trackEntry = animationState.SetAnimation(0, animationName, loop);
            if (trackEntry == null)
                return;

            _duration = trackEntry.Animation.Duration;
        }
    }
}

