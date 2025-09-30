using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using VContainer;

namespace Creature.Action
{
    public interface IAct
    {
        void Execute();
        void SetIsEnd(bool isEnd);
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
        [Inject] protected IObjectResolver _iResolver = null;

        protected T _param = null;
        protected IActor _iActor = null;
        protected System.Action<IActor> _endAction = null;
        protected float _duration = 0;
        protected bool _isActivate = false;
        protected bool _isEnd = false;
        
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

        protected virtual void End()
        {
            if (_isEnd)
                return;
            
            _endAction?.Invoke(_iActor);
            SetIsEnd(true);
        }

        #region IAct
        public abstract void Execute();
        
        public void SetIsEnd(bool isEnd)
        {
            _isEnd = isEnd;
        }
        
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

        protected bool PlayAnimation(string animationName, bool loop)
        {
            var skeletonAnimation = _iActor?.SkeletonAnimation;
            if (skeletonAnimation == null)
                return false;

            return skeletonAnimation.PlayAnimation(animationName, loop, OnCompleted, out _duration);
        }
    }
}

