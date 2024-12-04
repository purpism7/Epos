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
                mouseWorldPos.z = 0;
                
                // var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                var hits = Physics2D.RaycastAll(mouseWorldPos, mainCamera.transform.forward);
                if (hits != null)
                {
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        var hit = hits[i];
                        if(!hit)
                            continue;

                        if (hit.transform.gameObject.layer == 9)
                            return;
                        // if (!hit.transform.name.Contains("Back_01"))
                        //     return;
                    }
                }
                
               
                MainManager.Get<IFieldManager>()?.MoveToTarget(mouseWorldPos);
            }
        }

        void Entities.IGeneric.ChainLateUpdate()
        {
            
        }
    }
}

