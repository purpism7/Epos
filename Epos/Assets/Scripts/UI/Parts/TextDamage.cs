using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

using UI;

namespace Parts
{
    public class TextDamage : PartWorld<TextDamage.Data>
    {
        public class Data : PartWorld<Data>.Data
        {
            public float Damage { get; private set; } = 0;

            public Data WithDamage(float damage)
            {
                Damage = damage;
                return this;
            }
        }
        
        [SerializeField] private TextMeshProUGUI damageTMP = null;

        public override void Initialize(Data data)
        {
            base.Initialize(data);
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            damageTMP?.SetText($"{data.Damage}");
            
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
            endPos.y += 70f;   
            
            // await rootRectTm.DOLocalMove(endPos, 1f).SetUpdate(true).SetEase(Ease.Linear);
            await rootRectTm.DOLocalMoveY(endPos.y, 0.6f).SetEase(Ease.OutBack);
            
            Deactivate();
        }
    }
}

