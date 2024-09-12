using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public interface IInputManager : IManager
    {
        
    }
    
    public class InputManager : Manager
    {
        private ICameraManager _iCameraMgr = null;
        
        public override IManagerGeneric Initialize()
        {
            _iCameraMgr = MainGameManager.Get<ICameraManager>();
            
            return this;
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (_iCameraMgr == null)
                return;

            var mainCamera = _iCameraMgr?.MainCamera;
            if (mainCamera == null)
                return;

            if (_iCameraMgr.IsMove)
                return;
            
            var mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("mouseWorldPos = " + mouseWorldPos);
                
            }
        }
    }
}

