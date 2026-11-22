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
        
        // 표의 키로 재생한다. 지금 앞뒤에 맞는 이름은 CharacterAnimator 가 고른다.
        // faceDirection 을 주면 그쪽을 먼저 본 뒤 한 번만 재생한다(이동 시작, 이후 #2 공격 대상 쪽 보기).
        protected bool PlayAnimation(AnimationKey animationKey, bool loop, Vector2? faceDirection = null)
        {
            var characterAnimator = _actor?.CharacterAnimator;
            if (characterAnimator == null)
                return false;

            return characterAnimator.Play(animationKey, loop, OnCompleted, faceDirection);
        }
    }
}

