using UnityEngine;
using System;

using Cysharp.Threading.Tasks;

using Common;
using GameSystem;
using UnityEngine.UI;
using VContainer;

namespace UI.Parts
{
    public class EmotionPart : PartWorld<EmotionPart.Param>
    {
        public class Param : PartParam
        {
            public EmotionType EmotionType { get; private set; } = EmotionType.None;

            public Param WithEmotionType(EmotionType emotionType)
            {
                EmotionType = emotionType;
                return this;
            }
        }

        [Inject] private ResourceManager _resourceManager = null;
        
        [SerializeField] private Image emotionImg = null;
        
        private void LateUpdate()
        {
            ChainLateUpdate();
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

            SetEmotionImg();
            await UniTask.Delay(TimeSpan.FromSeconds(3f));
            
            Deactivate();
        }

        private void SetEmotionImg()
        {
            if (_param == null)
                return;
            
            if (emotionImg == null)
                return;
            
            var atlasLoader = _resourceManager?.AtlasLoader;

            var spriteName = $"Img_Battle_{_param.EmotionType}";
            var sprite = atlasLoader?.GetCommonSprite(spriteName);
            if (sprite == null)
                return;
            
            emotionImg.sprite = sprite;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            Return();
        }
    }
}

