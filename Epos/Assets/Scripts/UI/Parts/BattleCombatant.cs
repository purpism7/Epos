using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Creature;
using GameSystem;

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
            
            data?.ICombatant?.SetEventHandler(OnRefreshCharacter);
            
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

        private void SetHpProgress(IStat iStat)
        {
            if (iStat == null)
                return;

            if (hpProgress == null)
                return;

            hpProgress.fillAmount = iStat.Get(Stat.EType.Hp) / iStat.Get(Stat.EType.MaxHp);
        }

        private void OnRefreshCharacter(IActor iActor)
        {
            SetHpProgress(iActor?.IStat);
        }
    }
}

