using UnityEngine;
using UnityEngine.InputSystem;

using Actor;

namespace InputStates
{
    public class FocusedInputParam : InputParam
    {
        
    }
    
    public class FocusedInput : InputState<FocusedInputParam>
    {
        private IActor _actor = null;
        
        protected override void Initialize(FocusedInputParam param)
        {
            base.Initialize(param);
        }
        
        public override void ChainUpdate()
        {
            if (_actor == null)
                return;
            
            base.ChainUpdate();
            
            var keyboard = Keyboard.current;
            // var mouse = Mouse.current;
            
            var moveAxis = ReadMoveAxis(keyboard);
            _actor?.ActionController?.HandleMoveInput(moveAxis);
        }

        public void SetFocusedActor(IActor actor)
        {
            _actor = actor;
        }
        
        private Vector2 ReadMoveAxis(Keyboard keyboard)
        {
            if (keyboard == null) 
                return Vector2.zero;

            Vector2 axis = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  axis.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) axis.x += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)    axis.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)  axis.y -= 1f;

            // 대각선이 더 빠르지 않도록
            return axis.sqrMagnitude > 1f ? axis.normalized : axis;
        }
    }
}
