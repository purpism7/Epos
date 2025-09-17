using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

using UI;
using Common;

namespace Parts
{
    public class CombatText : PartWorld<CombatText.Param>
    {
        public class Param : PartWorld<Param>.PartParam
        {
            public float Value { get; private set; } = 0;
            public EImpactType EImpactType { get; private set; } = EImpactType.None;

            public Param WithValue(float value)
            {
                Value = value;
                return this;
            }

            public Param WithEImpactType(EImpactType eImpactType)
            {
                EImpactType = eImpactType;
                return this;
            }
        }

        [Header("Damage")]
        [SerializeField] private RectTransform damageRootRectTm = null;
        [SerializeField] private TextMeshProUGUI damageTMP = null;

        [Header("Heal")]
        [SerializeField] private RectTransform healRootRectTm = null;
        [SerializeField] private TextMeshProUGUI healTMP = null;

        public override UniTask InitializeAsync(Param data)
        {
            base.InitializeAsync(data);

            AllDeactivateRootRecTm();

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            SetText();

            MoveAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            AllDeactivateRootRecTm();
            Return();
        }

        private void AllDeactivateRootRecTm()
        {
            Extensions.SetActive(damageRootRectTm, false);  
            Extensions.SetActive(healRootRectTm, false);
        }

        private void SetText()
        {
            if (_param == null)
                return;

            switch (_param.EImpactType)
            {
                case EImpactType.Damage:
                    {
                        damageTMP?.SetText($"{Mathf.Abs(_param.Value)}");
                        Extensions.SetActive(damageRootRectTm, true);
                    }
                    break;

                case EImpactType.Heal:
                    {
                        healTMP?.SetText($"{Mathf.Abs(_param.Value)}");
                        Extensions.SetActive(healRootRectTm, true);
                    }
                    break;
            }
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

