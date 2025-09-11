using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using GameSystem;

namespace UI
{
    public abstract class PartWorld<T> : Common.Component<T> where T : PartWorld<T>.PartParam
    {
        public class PartParam : Common.Param
        {
            public Transform TargetTm = null;
            public Vector2 Offset = Vector2.zero;
        }
        
        [SerializeField] protected RectTransform rootRectTm = null;

        [Inject] private ICameraManager _iCameraManager = null;
        [Inject] private UIManager _uiManager = null;

        private void LateUpdate()
        {
            ChainLateUpdate();
        }

        private void ChainLateUpdate()
        {
            if (!rootRectTm)
                return;

            Vector3? pos = null;
            if (_param?.TargetTm)
                pos = GetScreenPos(_param.TargetTm.position);
      
            if(pos != null)
                rootRectTm.anchoredPosition = pos.Value;
        }
    
        protected Vector3? GetScreenPos(Vector3 targetPos)
        {
            var camera = _iCameraManager?.MainCamera;
            if (camera == null)
                return null;
            
            var worldUIRootRectTm = _uiManager?.WorldUIRootRectTm;
            if (!worldUIRootRectTm)
                return null;
            
            var uiCamera = _uiManager?.UICamera;
            if (uiCamera == null)
                return null;

            targetPos.x += _param.Offset.x;
            targetPos.y += _param.Offset.y;

            var screenPos = camera.WorldToScreenPoint(targetPos);

            Vector2 localPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(worldUIRootRectTm, screenPos, uiCamera, out localPos);
            //localPos.y += _data.Height;
            
            return localPos;
        } 
    }
}

