using System.Collections;
using System.Collections.Generic;
using Creature;
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
            public ICombatant ICombatant = null;
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
            var iCombatant = _data?.ICombatant;
            if (iCombatant == null)
                return;
            
            if (combatantImg == null)
                return;
            
            var sprite = ResourceManager.Instance.AtlasLoader?.GetCharacterSprite($"s_{iCombatant.Id}");
            combatantImg.sprite = sprite;
        }

        private void SetHpProgress()
        {
            var iCombatant = _data?.ICombatant;
            if (iCombatant?.IStat == null)
                return;

            if (hpProgress == null)
                return;

            hpProgress.fillAmount = iCombatant.IStat.Get(Stat.EType.Hp) / iCombatant.IStat.Get(Stat.EType.MaxHp);
        }
    }
}

