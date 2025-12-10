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
        }

        [Serializable]
        public class TextData
        {
            public ImpactType impatType = ImpactType.None;
            public RectTransform damagedRootRectTr = null;
            public TextMeshProUGUI damagedTMP = null;
        }


        //[Header("Ally Damaged")]
        //[SerializeField] private RectTransform allyDamagedRootRectTm = null;
        //[SerializeField] private TextMeshProUGUI allyDamagedTMP = null;

        //[Header("Enemy Damaged")]
        //[SerializeField] private RectTransform enemyDamagedRootRectTm = null;
        //[SerializeField] private TextMeshProUGUI enemyDamagedTMP = null;

        //[Header("Heal")]
        //[SerializeField] private RectTransform healRootRectTm = null;
        //[SerializeField] private TextMeshProUGUI healTMP = null;

        [SerializeField] private TextData[] textDatas = null;

        public override UniTask InitializeAsync(Param data)
        {
            base.InitializeAsync(data);

            AllDeactivateRootRecTr();

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

            AllDeactivateRootRecTr();
            Return();
        }

        private void AllDeactivateRootRecTr()
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

            if(!textDatas.IsNullOrEmpty())
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

