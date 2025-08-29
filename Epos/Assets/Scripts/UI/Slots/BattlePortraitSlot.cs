using UnityEngine;
using UnityEngine.UI;

using Creature;
using GameSystem.Event;
using Common;
using UI.Parts;

namespace UI.Slots
{
    public class BattlePortraitSlot : Slot<BattlePortraitSlot.Param>
    {
        public class Param : Common.Param
        {
            public ICombatant ICombatant { get; private set; } = null;
            public EClass EClass { get; private set; } = EClass.None;

            public Param(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
            }
        }

        [SerializeField] private Image characterImg = null;
        [SerializeField] private Image classImg = null;

        [SerializeField] private HpProgress hpProgress = null;

        private IHpProgress _iHpProgress = null;

        public override void Initialize()
        {
            base.Initialize();

            hpProgress?.Initialize();
        }

        public override void Activate(Param param)
        {
            base.Activate(param);

            EventHandler.Add<SkillUseEventData>(OnSkillUse);
            EventHandler.Add<StatChangedEventData>(OnStatChanged);
            
            ApplyCombatantImage();
            ApplyClassImage();
            ActivateHpProgress();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<SkillUseEventData>(OnSkillUse);
            EventHandler.Remove<StatChangedEventData>(OnStatChanged);
        }

        private void ActivateHpProgress()
        {
            hpProgress?.Activate(
                new HpProgress.Param()
                .WithCombatant(_param?.ICombatant));

            _iHpProgress = hpProgress;
        }

        private void ApplyCombatantImage()
        {
            characterImg?.SetActive(false);
            
            if (_param == null)
                return;
            
            if (characterImg == null)
                return;
            
            var sprite = GameSystem.ResourceManager.Instance?.AtlasLoader?.GetCharacterSprite($"p_{_param.ICombatant.Id}");
            if (sprite == null)
                sprite = GameSystem.ResourceManager.Instance?.AtlasLoader?.GetCommonSprite("Img_Monster_Normal");

            characterImg.sprite = sprite;
            characterImg.SetActive(true);
        }

        private void ApplyClassImage()
        {
            classImg?.SetActive(false);
            
            if (_param == null)
                return;
            
            if (classImg == null)
                return;

            if (_param.EClass == EClass.None)
                return;

            var spriteName = $"Img_Class_{_param.EClass}";
            var sprite = GameSystem.ResourceManager.Instance?.AtlasLoader?.GetSprite("Common", spriteName);
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

            if (eventData.CharacterId != _param.ICombatant.Id)
                return;

            _iHpProgress?.UpdateHpProgress();
        }
    }
}

