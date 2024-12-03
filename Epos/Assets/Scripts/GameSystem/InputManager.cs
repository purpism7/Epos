using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;

namespace GameSystem
{
    public interface IInputManager : IManager
    {
        
    }
    
    public class InputManager : MonoBehaviour, IInputManager
    {
        private ICameraManager _iCameraMgr = null;
        
        public Entities.IGeneric Initialize()
        {
            _iCameraMgr = GetComponent<CameraManager>();
            
            return this;
        }

        void Entities.IGeneric.ChainUpdate()
        {
            if (_iCameraMgr == null)
                return;

            var mainCamera = _iCameraMgr?.MainCamera;
            if (mainCamera == null)
                return;

            if (_iCameraMgr.IsMove)
                return;
            
           
            if (Input.GetMouseButtonUp(0))
            {
                var mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                
                // var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.Raycast(mouseWorldPos, mainCamera.transform.forward);
                if (hit)
                {
                    // 임시 
                    if (!hit.transform.name.Contains("Back_01"))
                        return;
                }
                
                mouseWorldPos.z = 0;
                MainManager.Get<IFieldManager>()?.MoveToTarget(mouseWorldPos);
            }
        }

        void Entities.IGeneric.ChainLateUpdate()
        {
            
        }
    }
}

