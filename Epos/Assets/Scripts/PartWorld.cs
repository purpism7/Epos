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
            public float Height = 0f;
            public Vector3 HeadPos = Vector3.zero;
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

            //Vector3 worldPo = _data.TargetTm.position;// + Vector3.up;
            //targetPos.y += _data.HeadPos.y;
            //Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);

            //// 2. È­¸é ÁÂÇ¥ ¡æ UI ·ÎÄÃ ÁÂÇ¥ º¯È¯
            //if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //    uiParentCanvas,
            //    screenPos,
            //    uiCamera,
            //    out Vector2 localPos))
            //{
            //    uiTarget.anchoredPosition = localPos;
            //}


            var screenPos = camera.WorldToScreenPoint(targetPos);

            Vector2 localPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(worldUIRootRectTm, screenPos, uiCamera, out localPos);
            localPos.y += _data.Height;
            
            return localPos;
        } 
    }
}

