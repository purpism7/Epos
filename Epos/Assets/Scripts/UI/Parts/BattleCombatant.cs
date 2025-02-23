using System.Collections;
using System.Collections.Generic;
using System.Data;
using Creator;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Creature;
using GameSystem;
using Parts;

namespace UI.Parts
{
    public class BattleCombatant : Part<BattleCombatant.Data>
    {
        public class Data : UI.Component.Data
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

            var data = new Damage.Data
            {
                TargetTm = _data?.ICombatant?.Transform,
                Damage = iActor != null ? (int)iActor.IStat.Get(Stat.EType.Attack) : 0,
            };
            
            UICreator<Damage, Damage.Data>.Get?
                .SetData(data)
                .Create()?
                .Activate(data);
        }
    }
}

