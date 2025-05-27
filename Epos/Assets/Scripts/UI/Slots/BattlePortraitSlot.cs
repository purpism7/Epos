using UnityEngine;
using UnityEngine.UI;

using Creature;

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

        public override void Activate(Data data)
        {
            base.Activate(data);

            ApplyCombatantImage();
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
    }
}

