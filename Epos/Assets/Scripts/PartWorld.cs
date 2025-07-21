using System;
using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

namespace UI
{
    public class PartWorld<T> : Common.Component<T> where T : PartWorld<T>.Data
    {
        public class Data : Common.ComponentData
        {
            public Transform TargetTm = null;
            public Vector2 Offset = Vector2.zero;
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

            Vector3? pos = null;
            if (_data?.TargetTm)
                pos = GetScreenPos(_data.TargetTm.position);
            //else if (_data?.TargetPos != null)
            //    pos = GetScreenPos(_data.TargetPos.Value);

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

            targetPos.x += _data.Offset.x;
            targetPos.y += _data.Offset.y;

            var screenPos = camera.WorldToScreenPoint(targetPos);

            Vector2 localPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(worldUIRootRectTm, screenPos, uiCamera, out localPos);
            //localPos.y += _data.Height;
            
            return localPos;
        } 
    }
}

