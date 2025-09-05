using Common;
using Cysharp.Threading.Tasks;
using Datas.ScriptableObjects;
using GameSystem;
using GameSystem.Event;
using Parts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.Parts;
using UI.Slot;
using UnityEngine;
using VContainer;

namespace UI.Panels
{
    public class BattleForces : BaseView<BattleForces.Param>
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

        public override void CreatePresenter(IObjectResolver iResolver)
        {
            
        }

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            allyBattlePartyPart?.Initialize();
            enemyBattlePartyPart?.Initialize();

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            useSkillNameTMP?.SetText(string.Empty);

            ActivateAllyBattleParty();
            ActivateEnemyBattleParty();

            EventHandler.Add<SkillUseEventData>(OnSkillUse);

            return UniTask.CompletedTask;
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

            allyBattlePartyPart?.ActivateAsync(battlePartyPartData);
        }

        private void ActivateEnemyBattleParty()
        {
            var battlePartyPartData = new BattlePartyPart.Param()
                .WithParty(_param?.EnemyParty)
                .WithETeam(ETeam.Enemy);

            enemyBattlePartyPart?.ActivateAsync(battlePartyPartData);
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

