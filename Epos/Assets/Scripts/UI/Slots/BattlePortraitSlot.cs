using UnityEngine;
using UnityEngine.UI;

using Creature;
using GameSystem.Event;

namespace UI.Slots
{
    public class BattlePortraitSlot : Slot<BattlePortraitSlot.Data>
    {
        public class Data : ComponentData
        {
            public int CharacterId { get; private set; } = 0;

            public Data WithCharacterId(int characterId)
            {
                CharacterId = characterId;
                return this;
            }
        }

        [SerializeField] private Image characterImg = null;
        [SerializeField] private Slider hpSlider = null;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            EventHandler.Add<SkillUseEventData>(OnSkillUse);

            ApplyCombatantImage();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<SkillUseEventData>(OnSkillUse);
        }

        private void ApplyCombatantImage()
        {
            if (_data == null)
                return;
            
            if (characterImg == null)
                return;
            
            var sprite = GameSystem.ResourceManager.Instance?.AtlasLoader?.GetCharacterSprite($"p_{_data.CharacterId}");
            characterImg.sprite = sprite;
        }

        private void OnSkillUse(SkillUseEventData eventData)
        {
            
        }
    }
}

