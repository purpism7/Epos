using System;
using UnityEngine;
using UnityEngine.UI;

using VContainer;
using Cysharp.Threading.Tasks;

using Creature;
using GameSystem.Event;
using Common;
using UI.Parts;
using EventHandler = GameSystem.Event.EventHandler;

namespace UI.Slot
{
    public interface IBattlePortraitSlot
    {
        UniTask UpdateEmotionAsync(EmotionType emotionType);
    }
    
    public class BattlePortraitSlot : BaseSlot<BattlePortraitSlot.Param>, IBattlePortraitSlot
    {
        public class Param : Common.Param
        {
            public ICombatant ICombatant { get; private set; } = null;
            public EClass EClass { get; private set; } = EClass.Knight;

            public Param(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
            }
        }

        [SerializeField] private Animator animator = null;
        [SerializeField] private Image characterImg = null;
        [SerializeField] private Image classImg = null;
        [SerializeField] private Image skillExpressionImg = null;
        [SerializeField] private HpProgress hpProgress = null;

        [Inject] private GameSystem.ResourceManager _resourceManager = null;

        private IHpProgress _iHpProgress = null;
        private IPortraitEmotionEmoji[] _emotionEmojis = null;
        
        public override async UniTask InitializeAsync(Param param = null)
        {
            await base.InitializeAsync(param);

            var skillExpressionSprite = _resourceManager?.AtlasLoader?.GetCharacterSprite($"p_{param?.ICombatant?.IActor.Id}_shout");
            if(skillExpressionSprite != null)
                skillExpressionImg.sprite = skillExpressionSprite;
            
            _emotionEmojis = GetComponentsInChildren<IPortraitEmotionEmoji>();
            if(_emotionEmojis != null)
            {
                foreach (var emotionEmoji in _emotionEmojis)
                {
                    if(emotionEmoji == null)
                        continue;

                    var actor = param?.ICombatant?.IActor;
                    if(actor == null)
                        continue;
                    
                    var portraitEmotionEmojiParam = new PortraitEmotionEmoji.Param(actor.Id)
                        .WithAtlasLoader(_resourceManager?.AtlasLoader);
                    
                    await emotionEmoji.InitializeAsync(portraitEmotionEmojiParam);
                }
            }
            
            hpProgress?.Initialize();
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            EventHandler.Add<SkillUseEventData>(OnSkillUse);
            EventHandler.Add<StatChangedEventData>(OnStatChanged);

            SetCombatantImage();
            SetClassImage();
            ActivateHpProgress();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<SkillUseEventData>(OnSkillUse);
            EventHandler.Remove<StatChangedEventData>(OnStatChanged);
        }

        private void ActivateHpProgress()
        {
            var param = new HpProgress.Param();
            param.WithCombatant(_param?.ICombatant);
            
            hpProgress?.ActivateAsync(param);

            _iHpProgress = hpProgress;
        }

        private void SetCombatantImage()
        {
            characterImg?.SetActive(false);
            
            if (_param == null)
                return;
            
            if (characterImg == null)
                return;

            var atlasLoader = _resourceManager?.AtlasLoader;
            var spriteName = $"p_{_param.ICombatant.IActor.Id}";
            var sprite = atlasLoader?.GetCharacterSprite(spriteName);

            characterImg.sprite = sprite;
            characterImg.SetActive(true);
        }

        private void SetClassImage()
        {
            classImg?.SetActive(false);
            
            if (_param == null)
                return;
            
            if (classImg == null)
                return;

            if (_param.EClass == EClass.None)
                return;

            var spriteName = $"Img_Class_{_param.EClass}";
            var sprite = _resourceManager?.AtlasLoader?.GetCommonSprite(spriteName);
            classImg.sprite = sprite;
            
            classImg?.SetActive(true);
        }
        

        private void OnSkillUse(SkillUseEventData eventData)
        {
            
        }

        private void OnStatChanged(StatChangedEventData eventData)
        {
            if (eventData == null ||
                _param == null)
                return;

            if (eventData.CharacterId != _param.ICombatant.IActor.Id)
                return;

            _iHpProgress?.UpdateHpProgress();
        }
        
        #region IBattlePortraitSlot

        async UniTask IBattlePortraitSlot.UpdateEmotionAsync(EmotionType emotionType)
        {
            if (_param?.ICombatant?.IActor is IEmotionalActor emotionalActor)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(2f));
                
                for (int i = 0; i < _emotionEmojis?.Length; ++i)
                {
                    var emotionEmoji = _emotionEmojis[i];
                    if(emotionEmoji == null)
                        continue;

                    if (emotionEmoji.EmotionType == emotionType)
                        await emotionEmoji.ActivateAsync(null);
                }
            }
            
            // Debug.Log(emotionType);
        }
        #endregion
    }
}

