using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using Entities;
using GameSystem;
using UI.Panels;
using Creator;
using Creature;
using Spine;

namespace Battle.Step
{
    public class BattleStart : BattleStep<BattleStart.Param>
    {
        [Inject] private UIManager _uiManager = null;
        [Inject] private UIFactory _uiFactory = null;
        
        public class Param : BattleStepParam
        {
            // public Datas.ScriptableObjects.Party AllyParty { get; private set; } = null;
            // public Datas.ScriptableObjects.Party EnemyParty { get; private set; } = null;

            public List<ICombatant> AllyICombatantList { get; private set; } = null;

            public Param(List<ICombatant> allyICombatantList)
            {
                AllyICombatantList = allyICombatantList;
                // AllyParty = allyParty;
                // EnemyParty = enemyParty;
            }
        }

        private UI.Popup.BattleStart _battleStart = null;

        public override void Begin()
        {
             ActivateBattleStartAsync().Forget();
        }

        private async UniTask ActivateBattleStartAsync()
        {
            var rootRectTm = _uiManager?.CurrViewRectTm;
            var uiCreator = _uiFactory?.Create<UI.Popup.BattleStart, UI.Popup.BattleStart.Param>();

            var battleStartParam = new UI.Popup.BattleStart.Param()
                .WithCompletedAction(OnCompletedBattleStart);

            _battleStart = await uiCreator
               .SetRoot(rootRectTm)
               .CreateAsync();
            _battleStart?.ActivateAsync(battleStartParam);
        }

        private void OnCompletedBattleStart(TrackEntry trackEntry)
        {
            _battleStart?.Deactivate();

            End();
        }
    }
}

