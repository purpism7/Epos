using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Datas.ScriptableObjects;
using GameSystem;
using Parts;
using UI.Parts;
using UI.Slots;
using Common;
using GameSystem.Event;
using TMPro;

namespace UI.Panels
{
    public class BattleForces : UI.Panel<BattleForces.Data>
    {
        public class Data : Common.ComponentData
        {
            public Party AllyParty { get; private set; } = null;
            public Party EnemyParty { get; private set; } = null;

            public Data(Party allyParty, Party enemyParty)
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
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);

            allyBattlePartyPart?.Initialize();
            enemyBattlePartyPart?.Initialize();
        }

        public override void Activate(Data data)
        {
            base.Activate(data);
            
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
            var battlePartyPartData = new BattlePartyPart.Data();
            battlePartyPartData
                .WithParty(_data?.AllyParty)
                .WithETeam(ETeam.Ally);
            allyBattlePartyPart?.Activate(battlePartyPartData);
        }

        private void ActivateEnemyBattleParty()
        {
            var battlePartyPartData = new BattlePartyPart.Data();
            battlePartyPartData
                .WithParty(_data?.EnemyParty)
                .WithETeam(ETeam.Enemy);
            enemyBattlePartyPart?.Activate(battlePartyPartData);
        }

        private void OnSkillUse(SkillUseEventData eventData)
        {
            useSkillNameTMP?.SetText(string.Empty);
            
            if (eventData?.Skill == null ||
                _data == null)
                return;
            
            // if (eventData.ETeam != _data.ETeam)
            //     return;
            
            Debug.Log(eventData.Skill.name);
            useSkillNameTMP?.SetText(eventData.Skill?.name);
        }
    }
}

