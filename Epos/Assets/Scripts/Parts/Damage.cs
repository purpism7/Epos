using System;
using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

namespace Parts
{
    public class Damage : Part<Damage.Data>
    {
        public class Data : BaseData
        {
            public Transform TargetTm = null;
            public int Damage = 0;
        }

        public override void Initialize(Data data)
        {
            base.Initialize(data);
        }

        public override void Activate(Data data)
        {
            base.Activate(data);
        }

        private void Update()
        {
            if (!_data?.TargetTm)
                return;
            
            var mainCamera = MainManager.Get<ICameraManager>()?.MainCamera;
            if (mainCamera == null)
                return;

            var screenPos = mainCamera.WorldToScreenPoint(_data.TargetTm.position);
            Vector2 viewPos = mainCamera.ScreenToViewportPoint(screenPos);
            viewPos -= Vector2.one * 0.5f;

            transform.position = new Vector2(viewPos.x, viewPos.y);
        }
    }
}

