using System.Collections;
using System.Collections.Generic;
using System.Data;
using Creator;
using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using TMPro;

using Creature;
using GameSystem;
using Parts;

namespace UI.Parts
{
    public class BattleCombatant : Part<BattleCombatant.Param>
    {
        public class Param : Common.Param
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

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);
            
            // data?.ICombatant?.Add(OnChanged);
            
            SetCombatantImg();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // _data.ICombatant?.Remove(OnChanged);
        }

        private void SetCombatantImg()
        {
            var iCombatant = _param?.ICombatant;
            if (iCombatant == null)
                return;
            
            if (combatantImg == null)
                return;
            
            // var sprite = ResourceManager.Instance?.AtlasLoader?.GetCharacterSprite($"s_{iCombatant.Id}");
            // combatantImg.sprite = sprite;
        }

        private void SetHpProgress(IStat iStat)
        {
            if (iStat == null)
                return;

            if (hpProgress == null)
                return;

            float hp = iStat.Get(Stat.EType.Hp);
            float maxHp = iStat.Get(Stat.EType.MaxHp);
            
            hpProgress.fillAmount = hp / maxHp;
        }

        // private void OnChanged(DamageEventData damageEventData)
        // {
        //     // var data = new TextDamage.Data
        //     // {
        //     //     TargetTm = _data?.ICombatant?.Transform,
        //     //     Damage = iActor != null ? iActor.IStat.Get(Stat.EType.Attack) : 0,
        //     // };
        //     //
        //     // UICreator<TextDamage, TextDamage.Data>.Get?
        //     //     .SetData(data)
        //     //     .Create()?
        //     //     .Activate(data);
        //     //
        //     // SetHpProgress(iActor?.IStat);
        // }
    }
}

