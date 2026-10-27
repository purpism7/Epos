using System;
using System.Collections.Generic;
using UnityEngine;

using Characters;
using Commons;
using InputStates;

namespace Managers
{
    public class InputManager : Entity
    {
        private IInputState _inputState = null;

        private CharacterManager _characterManager = null;
        
        public void Initialize(CharacterManager characterManager)
        {
            Debug.Log("InputManager Initialize");

            // 임시
            IInputState inputState = new FocusedInput();
            inputState.Initialize(new FocusedInputParam());
            SetInputState(inputState);

            _characterManager = characterManager;
        }

        public override void Activate()
        {
            if (_characterManager == null)
                return;

            _characterManager.OnMainPlayerChanged -= OnMainPlayerChanged;
            _characterManager.OnMainPlayerChanged += OnMainPlayerChanged;
            
            base.Activate();
        }

        public override void Deactivate()
        {
            if (_characterManager != null)
                _characterManager.OnMainPlayerChanged -= OnMainPlayerChanged;
            
            base.Deactivate();
        }

        public override void ChainUpdate()
        {
            if (_inputState == null)
                return;
            
            _inputState.ChainUpdate();
        }

        public void SetInputState(IInputState inputState)
        {
            if (inputState == null) 
                return;

            // var key = inputState.GetType();
            
            // 1. 이미 같은 타입의 상태가 캐시 풀에 있다면 새것을 버리고 기존 캐시를 재사용 (메모리 절약)
            // if (_inputStates.TryGetValue(key, out IInputState cachedState))
            // {
            //     inputState = cachedState;
            // }
            // else
            // {
            //     // 2. 처음 들어온 상태라면 캐시 풀에 등록
            //     _inputStates.Add(key, inputState);
            // }
            
            // 3. 인터페이스에 정의된 Initialize() 호출 (컴파일 에러 완벽 해결)
            // inputState.Initialize();
            
            _inputState = inputState;
        }

        private void OnMainPlayerChanged(PlayerCharacter player)
        {
            if (_inputState is FocusedInput focusedInput)
                focusedInput.SetFocusedActor(player);
        }
    }
}

