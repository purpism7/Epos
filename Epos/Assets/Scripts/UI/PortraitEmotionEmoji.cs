using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

using Common;
using Creature;
using GameSystem;
using VContainer;

namespace UI
{
    public interface IPortraitEmotionEmoji
    {
        EmotionType EmotionType { get; }

        UniTask InitializeAsync(PortraitEmotionEmoji.Param param = null);
        UniTask ActivateAsync(PortraitEmotionEmoji.Param param);
    }
    
    public class PortraitEmotionEmoji : BaseSlot<PortraitEmotionEmoji.Param>, IPortraitEmotionEmoji
    {
        public class Param : Common.Param
        {
            public int CharacterId { get; private set; } = 0;
            public AtlasLoader AtlasLoader { get; private set; } = null;

            public Param(int characterId)
            {
                CharacterId = characterId;
            }
            
            public Param WithAtlasLoader(AtlasLoader loader)
            {
                AtlasLoader = loader;
                return this;
            }
        }

        [SerializeField] private EmotionType emotionType = EmotionType.None;
        [SerializeField] private Image characterImg = null;
        [SerializeField] private Animation animation = null;

        public override async UniTask InitializeAsync(Param param = null)
        {
            await base.InitializeAsync(param);

            SetCharacterImg();
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

            if (animation?.clip != null)
                await UniTask.Delay(TimeSpan.FromSeconds(animation.clip.length + 0.1f));
            
            Deactivate();
        }

        private void SetCharacterImg()
        {
            characterImg?.SetActive(false);
            
            if (_param == null)
                return;

            if (characterImg == null)
                return;
            
            var sprite = _param?.AtlasLoader?.GetCharacterSprite($"p_{_param.CharacterId}_{emotionType}");
            characterImg.sprite = sprite;
            
            characterImg.SetActive(true);
        }
        
        #region IPortraitEmotionEmoji
        EmotionType IPortraitEmotionEmoji.EmotionType => emotionType;
        #endregion
    }
}

