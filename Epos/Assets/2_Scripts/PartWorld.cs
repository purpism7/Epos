using System;
using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

namespace UI
{
    public class PartWorld<T> : UI.Component<T> where T : PartWorld<T>.Data
    {
        public class Data : UI.Component.Data
        {
            public Transform TargetTm = null;
            public float Height = 200f;
        }
        
        [SerializeField] protected RectTransform rootRectTm = null;

        private void LateUpdate()
        {
            // ChainLateUpdate();
        }

        protected virtual void ChainLateUpdate()
        {
            if (!rootRectTm)
                return;

            if (!_data?.TargetTm)
                return;

            var pos = GetScreenPos(_data.TargetTm.position);
            if(pos != null)
                rootRectTm.anchoredPosition = pos.Value;
        }
    
        protected Vector3? GetScreenPos(Vector3 targetPos)
        {
            var camera = MainManager.Get<ICameraManager>()?.MainCamera;
            if (camera == null)
                return null;
            
            var worldUIRootRectTm = UIManager.Instance?.WorldUIRootRectTm;
            if (!worldUIRootRectTm)
                return null;
            
            var uiCamera = UIManager.Instance?.UICamera;
            if (uiCamera == null)
                return null;
            
            var screenPos = camera.WorldToScreenPoint(targetPos);

            Vector2 localPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(worldUIRootRectTm, screenPos, uiCamera, out localPos);
            localPos.y += _data.Height;
            
            return localPos;
        } 
    }
}

