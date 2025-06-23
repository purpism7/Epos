using UnityEngine;
using UnityEngine.UI;

using Creature;
using GameSystem.Event;
using Common;

namespace UI.Slots
{
    public class BattlePortraitSlot : Slot<BattlePortraitSlot.Data>
    {
        public class Data : Common.ComponentData
        {
            public int CharacterId { get; private set; } = 0;
            public EClass EClass { get; private set; } = EClass.None;
            
            public Data(int characterId, EClass eClass)
            {
                CharacterId = characterId;
                EClass = eClass;
            }
        }

        [SerializeField] private Image characterImg = null;
        [SerializeField] private Slider hpSlider = null;
        [SerializeField] private Image classImg = null;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            EventHandler.Add<SkillUseEventData>(OnSkillUse);
            EventHandler.Add<StatChangedEventData>(OnStatChanged);

            ApplyCombatantImage();
            ApplyClassImage();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<SkillUseEventData>(OnSkillUse);
            EventHandler.Remove<StatChangedEventData>(OnStatChanged);
        }

        private void ApplyCombatantImage()
        {
            characterImg?.SetActive(false);
            
            if (_data == null)
                return;
            
            if (characterImg == null)
                return;
            
            var sprite = GameSystem.ResourceManager.Instance?.AtlasLoader?.GetCharacterSprite($"p_{_data.CharacterId}");
            characterImg.sprite = sprite;
            
            characterImg?.SetActive(true);
        }

        private void ApplyClassImage()
        {
            classImg?.SetActive(false);
            
            if (_data == null)
                return;
            
            if (classImg == null)
                return;

            var spriteName = $"Img_Class_{_data.EClass}";
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
                _data == null)
                return;

            if (eventData.CharacterId != _data.CharacterId)
                return;
            
            var iStat = eventData.IStat;
            var hp = iStat.Get(Stat.EType.Hp);
            var maxHp = iStat.Get(Stat.EType.MaxHp);
            Debug.Log($"{hp}/{maxHp}");
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHp;
                hpSlider.SetValueWithoutNotify(hp);
               
            }
           

        }
    }
}

