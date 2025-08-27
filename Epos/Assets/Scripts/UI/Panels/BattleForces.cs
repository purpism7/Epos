using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using Datas.ScriptableObjects;
using GameSystem;
using Parts;
using UI.Parts;
using UI.Slots;
using Common;
using GameSystem.Event;

namespace UI.Panels
{
    public class BattleForces : UI.Panel<BattleForces.Param>
    {
        public class Param : Common.Param
        {
            public Party AllyParty { get; private set; } = null;
            public Party EnemyParty { get; private set; } = null;

            public Param(Party allyParty, Party enemyParty)
            {
                AllyParty = allyParty;
                EnemyParty = enemyParty;
            }
        }

        [SerializeField] private TextMeshProUGUI useSkillNameTMP = null;
        
        [Header("Ally")]
        [SerializeField] private BattlePartyPart allyBattlePartyPart = null;
        
        [Header("Enemy")]
        [SerializeField] private BattlePartyPart enemyBattlePartyPart = null;
        
        public override void Initialize(Param param)
        {
            base.Initialize(param);

            allyBattlePartyPart?.Initialize();
            enemyBattlePartyPart?.Initialize();
        }

        public override void Activate(Param param)
        {
            base.Activate(param);
            
            useSkillNameTMP?.SetText(string.Empty);
            
            ActivateAllyBattleParty();
            ActivateEnemyBattleParty();
            
            EventHandler.Add<SkillUseEventData>(OnSkillUse);
        }
        
        public override void Deactivate()
        {
            base.Deactivate();

            allyBattlePartyPart?.Deactivate();
            enemyBattlePartyPart?.Deactivate();
            
            EventHandler.Remove<SkillUseEventData>(OnSkillUse);
        }

        private void ActivateAllyBattleParty()
        {
            var battlePartyPartData = new BattlePartyPart.Param()
                .WithParty(_param?.AllyParty)
                .WithETeam(ETeam.Ally);

            allyBattlePartyPart?.Activate(battlePartyPartData);
        }

        private void ActivateEnemyBattleParty()
        {
            var battlePartyPartData = new BattlePartyPart.Param()
                .WithParty(_param?.EnemyParty)
                .WithETeam(ETeam.Enemy);

            enemyBattlePartyPart?.Activate(battlePartyPartData);
        }

        private void OnSkillUse(SkillUseEventData eventData)
        {
            useSkillNameTMP?.SetText(string.Empty);
            
            if (eventData?.Skill == null ||
                _param == null)
                return;
            
            // if (eventData.ETeam != _data.ETeam)
            //     return;
            
            Debug.Log(eventData.Skill.name);
            useSkillNameTMP?.SetText(eventData.Skill?.name);
        }
    }
}

