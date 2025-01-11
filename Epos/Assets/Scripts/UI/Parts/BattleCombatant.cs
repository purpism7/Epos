using System.Collections;
using System.Collections.Generic;
using GameSystem;
using TMPro;
using UnityEngine;

using UnityEngine.UI;

namespace UI.Parts
{
    public class BattleCombatant : Part<BattleCombatant.Data>
    {
        public class Data : BaseData
        {
            public int Id = 0;
        }
        
        [SerializeField] 
        private int position = 0;
        [SerializeField] 
        private TextMeshProUGUI levelTMP = null;
        [SerializeField] 
        private Image combatantImg = null;
        [SerializeField] 
        private Image hpProgress = null;

        public override void Initialize(Data data)
        {
            base.Initialize(data);

            SetCombatantImg();
        }

        private void SetCombatantImg()
        {
            if (_data == null)
                return;
            
            if (combatantImg == null)
                return;

            var sprite = ResourceManager.Instance.AtlasLoader?.GetCharacterSprite($"s_{_data.Id}");
            combatantImg.sprite = sprite;
        }
    }
}

