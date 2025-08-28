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
    public class TextDamage : PartWorld<TextDamage.Param>
    {
        public class Param : PartWorld<Param>.PartParam
        {
            public float Damage { get; private set; } = 0;

            public Param WithDamage(float damage)
            {
                Damage = damage;
                return this;
            }
        }
        
        [SerializeField] private TextMeshProUGUI damageTMP = null;

        public override UniTask InitializeAsync(Param data)
        {
            base.InitializeAsync(data);

            return UniTask.CompletedTask;
        }

        public override void Activate(Param param)
        {
            base.Activate(param);

            damageTMP?.SetText($"{param.Damage}");
            
            MoveAsync().Forget();
        }

        private async UniTask MoveAsync()
        {
            if (!rootRectTm)
                return;

            if (!_param?.TargetTm)
                return;
            
            var startPos = GetScreenPos(_param.TargetTm.position);
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

