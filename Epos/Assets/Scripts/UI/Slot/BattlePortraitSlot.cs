using System;
using UnityEngine;
using UnityEngine.UI;

using VContainer;
using Cysharp.Threading.Tasks;

using Creature;
using Creature.Actions;
using GameSystem.Event;
using Datas.ScriptableObjects;
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
        private IActor _actor = null;
        
        public override async UniTask InitializeAsync(Param param = null)
        {
            await base.InitializeAsync(param);

            // IActor 캐싱 (깊은 체인 접근 방지)
            _actor = param?.ICombatant?.Actor;

            var skillExpressionSprite = _resourceManager?.AtlasLoader?.GetCharacterSprite($"p_{_actor?.Id}_Shout");
            if(skillExpressionSprite != null)
                skillExpressionImg.sprite = skillExpressionSprite;
            
            _emotionEmojis = GetComponentsInChildren<IPortraitEmotionEmoji>();
            if(_emotionEmojis != null && _actor != null)
            {
                foreach (var emotionEmoji in _emotionEmojis)
                {
                    if(emotionEmoji == null)
                        continue;
                    
                    var portraitEmotionEmojiParam = new PortraitEmotionEmoji.Param(_actor.Id)
                        .WithAtlasLoader(_resourceManager?.AtlasLoader);
                    
                    await emotionEmoji.InitializeAsync(portraitEmotionEmojiParam);
                }
            }
            
            hpProgress?.Initialize();
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);


            EventHandler.Add<StatChangedEventData>(OnStatChanged);

            // ActController 직접 구독 (특정 캐릭터만 관심있으므로 직접 구독이 더 효율적)
            RegisterActControllerEvents();

            SetCombatantImage();
            SetClassImage();
            ActivateHpProgress();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<StatChangedEventData>(OnStatChanged);

            // ActController 이벤트 해제
            UnregisterActControllerEvents();
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
            
            if (_actor == null || characterImg == null)
                return;

            var atlasLoader = _resourceManager?.AtlasLoader;
            var spriteName = $"p_{_actor.Id}";
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
        

        private void OnStatChanged(StatChangedEventData eventData)
        {
            if (eventData == null || _actor == null)
                return;

            if (eventData.CharacterId != _actor.Id)
                return;

            _iHpProgress?.UpdateHpProgress();
        }

        private void RegisterActControllerEvents()
        {
            // InitializeAsync에서 이미 캐싱된 IActController 사용
            var actController = _actor?.ActController;
            if (actController == null)
            {
                Debug.LogWarning($"BattlePortraitSlot: IActController is null. CharacterId: {_actor?.Id}");
                return;
            }

            // 모든 Act 변경 이벤트 등록 (IAct로 구독하면 모든 Act 타입에 대해 이벤트를 받음)
            actController.OnActStarted<IAct>(OnActStarted);
            actController.OnActEnded<IAct>(OnActEnded);
        }

        private void UnregisterActControllerEvents()
        {
            var actController = _actor?.ActController;
            if (actController == null)
                return;

            // 등록한 이벤트 해제
            actController.RemoveActStarted<IAct>(OnActStarted);
            actController.RemoveActEnded<IAct>(OnActEnded);
            
            _actor = null;
        }

        private void OnActStarted(IAct act)
        {
            if (act == null || _actor == null)
                return;

            // 모든 Act가 시작될 때 처리할 로직
            //Debug.Log($"BattlePortrait: {act.GetType().Name} 액션이 시작되었습니다. CharacterId: {_iActor.Id}");
            
            // Act 타입별 처리
            switch (act)
            {
                case Die dieAct:
                    // Die Act 처리
                    OnDieActStarted(dieAct);
                    break;
                case Move moveAct:
                    // Move Act 처리
                    // OnMoveActStarted(moveAct);
                    break;
                case Casting castingAct:
                    // Casting Act 처리
                    OnCastingActStarted(castingAct);
                    break;
                // 다른 Act 타입도 필요하면 추가
            }
        }

        private void OnActEnded(IAct act)
        {
            if (act == null || _actor == null)
                return;

            // 모든 Act가 종료될 때 처리할 로직
            //Debug.Log($"BattlePortrait: {act.GetType().Name} 액션이 종료되었습니다. CharacterId: {_iActor.Id}");
            
            // Act 타입별 처리
            switch (act)
            {
                case Die dieAct:
                    // Die Act 처리
                    OnDieActEnded(dieAct);
                    break;
                case Move moveAct:
                    // Move Act 처리
                    // OnMoveActEnded(moveAct);
                    break;
                case Casting castingAct:
                    // Casting Act 처리
                    OnCastingActEnded(castingAct);
                    break;
                // 다른 Act 타입도 필요하면 추가
            }
        }

        private void OnDieActStarted(Die dieAct)
        {
            // Die Act가 시작될 때 처리할 로직
            // 예: UI 업데이트, 애니메이션 재생 등
        }

        private void OnDieActEnded(Die dieAct)
        {
            // Die Act가 종료될 때 처리할 로직
            // 예: UI 업데이트, 애니메이션 재생 등
        }

        private void OnCastingActStarted(Casting castingAct)
        {
            if (castingAct == null)
                return;

            // Casting Act가 시작될 때 스킬 정보 확인
            var iSkill = castingAct.ISkill;
            var skillData = castingAct.SkillData;

            if (skillData != null)
            {
                //Debug.Log($"BattlePortrait: 스킬 사용 시작 - SkillId: {skillData.Id}, SkillName: {skillData.GetType()}, CharacterId: {_iActor?.Id}");
                // 예: 스킬 아이콘 표시, 스킬 이름 표시 등
                //skillExpressionImg?.SetActive(true);
                animator?.SetBool("SkillUse", true);


            }
        }

        private void OnCastingActEnded(Casting castingAct)
        {
            if (castingAct == null)
                return;

            // Casting Act가 종료될 때 처리할 로직
            var skillData = castingAct.SkillData;
            if (skillData != null)
            {
                //Debug.Log($"BattlePortrait: 스킬 사용 종료 - SkillId: {skillData.Id}, CharacterId: {_iActor?.Id}");
                // 예: UI 업데이트, 애니메이션 재생 등
                animator?.SetBool("SkillUse", false);
            }
        }
        
        #region IBattlePortraitSlot

        async UniTask IBattlePortraitSlot.UpdateEmotionAsync(EmotionType emotionType)
        {
            if (_actor is IEmotionalActor emotionalActor)
            {
                if (emotionalActor.Id != 10003)
                    return;
                
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
                
                for (int i = 0; i < _emotionEmojis?.Length; ++i)
                {
                    var emotionEmoji = _emotionEmojis[i];
                    if(emotionEmoji == null)
                        continue;

                    if (emotionEmoji.EmotionType == emotionType)
                        await emotionEmoji.ActivateAsync(null);
                }
            }
        }
        #endregion
    }
}

