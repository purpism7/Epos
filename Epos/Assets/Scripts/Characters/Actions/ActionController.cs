using System.Collections.Generic;
using UnityEngine;

using Actor;

namespace Characters.Actions
{
    public interface IActionController
    {
        void Flip(float x);
        // void MoveByAxis(Vector2 moveAxis);
        void HandleMoveInput(Vector2 moveAxis);
    }
    
    public class ActionController : IActionController
    {
        private IActor _actor = null;
        private Action _currentAction = null;
        
        private Dictionary<System.Type, Action> _actions = new Dictionary<System.Type, Action>();

        public ActionController(IActor actor)
        {
            _actions?.Clear();
            
            _actor = actor;
        }

        public void Activate()
        {
            
        }

        public void Deactivate()
        {
            
        }

        public void ChainUpdate()
        {
            _currentAction?.ChainUpdate();
        }
        
        #region IActionController

        void IActionController.Flip(float x)
        {
            // 좌우 반전은 CharacterAnimator 한 곳에서만 한다(그림 기본 방향 반영, 문턱 0.01 동일, 앞뒤는 그대로).
            _actor?.CharacterAnimator?.Face(new Vector2(x, 0f));
        }

        void IActionController.HandleMoveInput(Vector2 moveAxis)
        {
            if (moveAxis != Vector2.zero)
            {
                var param = new MoveParam
                {
                    MoveAxis = moveAxis,
                };
            
                ExecuteAction<Move, MoveParam>(param);
            }
            else
            {
                if(_currentAction?.GetType() != typeof(Idle))
                    ExecuteAction<Idle, IdleParam>();
            }
        }
        #endregion

        private void ExecuteAction<TAction, TParam>(TParam param = null) 
            where TAction : Action<TParam>, new()
            where TParam : ActionParam
        {
            if (_actions == null)
                _actions = new();
            
            var actionKey = typeof(TAction);
            if (!_actions.TryGetValue(actionKey, out var action))
            {
                action = new TAction();
                action.SetActor(_actor)?
                    .SetEndAction(EndAction);
                
                _actions[actionKey] = action;
            }

            action.SetActionParam(param);
            
            if (_currentAction?.GetType() != typeof(TAction))
                action.Execute();

            _currentAction = action;
        }

        private void EndAction()
        {
            if (_currentAction is Move)
                ExecuteAction<Idle, IdleParam>();
        }
    }
}

