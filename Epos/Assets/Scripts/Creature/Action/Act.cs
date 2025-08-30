using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace Creature.Action
{
    public interface IAct
    {
        void Execute();
        void Deactivate();
        
        void ChainUpdate();
        void ChainFixedUpdate();
    }

    public class ActParam
    {
        public string AnimationKey { get; private set; } = string.Empty;

        public ActParam SetAnimationKey(string key)
        {
            AnimationKey = key;

            return this;
        }
    }

    public abstract class Act<T> : IAct where T : ActParam
    {
        protected T _param = null;
        protected IActor _iActor = null;
        protected System.Action<IActor> _endAction = null;
        protected float _duration = 0;
        protected bool _isActivate = false;
        
        public virtual void Initialize(IActor iActor)
        {
            _iActor = iActor;
        }
        
        protected Act<T> SetIActor(IActor iActor)
        {
            _iActor = iActor;
            return this;
        }
        
        public void SetParam(T param)
        {
            _param = param;
        }

        public void SetEndActAction(System.Action<IActor> endAction)
        {
            _endAction = endAction;
        }
        
        #region IAct
        public abstract void Execute();

        protected virtual void Activate()
        {
            _isActivate = true;
        }
        
        public virtual void Deactivate()
        {
            _isActivate = false;
        }
        
        public virtual void ChainUpdate()
        {
            
        }

        public virtual void ChainFixedUpdate()
        {
            
        }
        #endregion

        protected virtual void OnCompleted(TrackEntry trackEntry)
        {
            
        }

        protected void SetAnimation(string animationName, bool loop)
        {

            _iActor?.SkeletonAnimation?.PlayAnimation(animationName, loop, OnCompleted, out _duration);
            // if (_iActor == null)
            //     return;
            
            // var animationState = _iActor.SkeletonAnimation?.AnimationState;
            // if (animationState == null)
            //     return;
            
            // var animation = _iActor.SkeletonAnimation.skeletonDataAsset?.GetSkeletonData(true)?.Animations?
            //     .Find(animation => animation.Name.Contains(animationName));
            // if (animation == null)
            //     return;
            
            // var trackEntry = animationState.SetAnimation(0, animationName, loop);
            // if (trackEntry == null)
            //     return;

            // trackEntry.Complete -= OnCompleted;
            // trackEntry.Complete += OnCompleted;

            // _duration = trackEntry.Animation.Duration;
        }
    }
}

