using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;

using GameSystem;
using UI;

namespace Parts
{
    public class Damage : PartWorld<Damage.Data>
    {
        public class Data : PartWorld<Data>.Data
        {
            // public Transform TargetTm = null;
            public int Damage = 0;
        }

        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            
        }
        
        public override void Activate(Data data)
        {
            base.Activate(data);

            MoveAsync().Forget();
        }

        private async UniTask MoveAsync()
        {
            if (!rootRectTm)
                return;

            if (!_data?.TargetTm)
                return;
            
            var startPos = GetScreenPos(_data.TargetTm.position);
            if (startPos == null) 
                return;
            
            rootRectTm.anchoredPosition = startPos.Value;
            var endPos = startPos.Value;
            endPos.y += 100f;   
            
            await rootRectTm.DOLocalMove(endPos, 1f).SetUpdate(true).SetEase(Ease.Linear);
            
            Deactivate();
        }
    }
}

