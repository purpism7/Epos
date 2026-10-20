using UnityEngine;

using Spine;

using Actor;

namespace Characters.Actions
{
    public class ActionParam
    {
        
    }
    
    public abstract class Action
    {
        protected IActor _actor = null;
        protected System.Action _endAction = null;
        
        public abstract void Execute();
        public abstract void ChainUpdate();
        
        public abstract Action SetActionParam(ActionParam param);

        public Action SetActor(IActor actor)
        {
            _actor = actor;
            return this;
        }

        public Action SetEndAction(System.Action endAction)
        {
            _endAction = endAction;
            return this;
        }
    }
    
    public abstract class Action<TParam> : Action where TParam : ActionParam
    {
        protected TParam _actionParam = null;

        public override Action SetActionParam(ActionParam param)
        {
            if (param is not TParam typedParam)
                return null;

            _actionParam = typedParam;

            return this;
        }
        
        public virtual void Activate()
        {
            
        }

        protected virtual void EndAction()
        {
            _endAction?.Invoke();
        }
        
        protected virtual void OnCompleted(TrackEntry trackEntry)
        {
            
        }
        
        protected bool PlayAnimation(string animationName, bool loop)
        {
            var skeletonAnimation = _actor?.SkeletonAnimation;
            if (skeletonAnimation == null)
                return false;

            return skeletonAnimation.PlayAnimation(animationName, loop, OnCompleted, out _);
        }
    }
}

