using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Manager;

namespace GameSystem
{
    public interface IInputManager : IManager
    {
        
    }
    
    public class InputManager : MonoBehaviour, IInputManager
    {
        private ICameraManager _iCameraMgr = null;
        
        public Manager.IGeneric Initialize()
        {
            _iCameraMgr = GetComponent<CameraManager>();
            
            return this;
        }

        void Manager.IGeneric.ChainUpdate()
        {
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
                mouseWorldPos.z = 0;
                MainManager.Get<IFieldManager>()?.MoveToTarget(mouseWorldPos);
            }
        }

        void Manager.IGeneric.ChainLateUpdate()
        {
            
        }
    }
}

