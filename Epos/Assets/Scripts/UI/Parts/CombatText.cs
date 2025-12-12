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
            public ImpactType ImpactType { get; private set; } = ImpactType.None;
            public float FlipX { get; private set; } = 0;

            public Param WithValue(float value)
            {
                Value = value;
                return this;
            }

            public Param WithImpactType(ImpactType impactType)
            {
                ImpactType = impactType;
                return this;
            }

            public Param WithFlipX(float flipX)
            {
                FlipX = flipX;
                return this;
            }
        }

        [Serializable]
        public class TextData
        {
            public ImpactType impatType = ImpactType.None;
            public RectTransform damagedRootRectTr = null;
            public TextMeshProUGUI damagedTMP = null;
        }

        [SerializeField] private TextData[] textDatas = null;

        public override UniTask InitializeAsync(Param data)
        {
            base.InitializeAsync(data);

            DeactivateAllRootRectTr();

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            ActivateText();

            MoveAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            DeactivateAllRootRectTr();
            Return();
        }

        private void DeactivateAllRootRectTr()
        {
            if (textDatas.IsNullOrEmpty())
                return;

            for(int i = 0; i < textDatas.Length; ++i)
            {
                Extensions.SetActive(textDatas[i]?.damagedRootRectTr, false);
            }
        }

        private void ActivateText()
        {
            if (_param == null)
                return;

            DeactivateAllRootRectTr();

            if (!textDatas.IsNullOrEmpty())
            {
                for (int i = 0; i < textDatas.Length; ++i)
                {
                    var textData = textDatas[i];
                    if (textData == null)
                        continue;

                    if(_param.ImpactType == textData.impatType)
                    {
                        textData.damagedTMP?.SetText($"{Mathf.Abs(_param.Value)}");
                        Extensions.SetActive(textData.damagedRootRectTr, true);

                        break;
                    }
                }
            }
        }

        private Vector3 CalculateStartPosition
        {
            get
            {
                var startPosition = GetScreenPos(_param.TargetTm.position);
                if (startPosition == null)
                    return Vector3.zero;

                Vector3 resStartPosition = startPosition.Value;
                if (_param.ImpactType == ImpactType.PhysicalDamage ||
                    _param.ImpactType == ImpactType.MagicalDamage)
                {
                    if (_param.FlipX >= 1)
                        resStartPosition.x -= 50f;
                    else
                        resStartPosition.x += 50f;
                }

                return resStartPosition;
            }
        }

        private Vector3 CalculateEndPosition(Vector3 startPosition)
        {
            var endPosition = startPosition;
            if (_param.ImpactType == ImpactType.PhysicalDamage ||
                _param.ImpactType == ImpactType.MagicalDamage)
            {
                if (_param.FlipX >= 1)
                    endPosition.x -= 80f;
                else
                    endPosition.x += 80f;
            }
          
            endPosition.y += 70f;

            return endPosition;
        }

        private async UniTask MoveAsync()
        {
            if (!rootRectTm)
                return;

            if (!_param?.TargetTm)
                return;

            var startPosition = CalculateStartPosition;
            var endPosition = CalculateEndPosition(startPosition);

            rootRectTm.anchoredPosition = startPosition;

            // await rootRectTm.DOLocalMove(endPos, 1f).SetUpdate(true).SetEase(Ease.Linear);
            await rootRectTm.DOLocalMove(endPosition, 0.6f).SetEase(Ease.OutBack);
            
            Deactivate();
        }
    }
}

