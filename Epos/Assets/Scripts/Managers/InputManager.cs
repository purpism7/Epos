using UnityEngine;

using InputStates;

namespace Managers
{
    public class InputManager : IManager
    {
        private InputState _inputState = null;
        
        public void Initialize()
        {
            Debug.Log("InputManager Initialize");
            
            // 임시
            SetInputState(new FocusedInput());
        }

        public void ChainUpdate()
        {
            if (_inputState == null)
                return;
            
            _inputState.ChainUpdate();
        }

        public void ChainLateUpdate()
        {
            
        }

        public void SetInputState(InputState inputState)
        {
            _inputState = inputState;
        }
    }
}

