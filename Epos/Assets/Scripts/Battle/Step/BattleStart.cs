using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Entities;
using GameSystem;
using UI.Panels;
using UI.Popups;
using Creator;
using Creature;
using Spine;

namespace Battle.Step
{
    public class BattleStart : BattleStep<BattleStart.Param>
    {
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

        private UI.Popups.BattleStart _battleStart = null;

        public override void Begin()
        {
             ActivateBattleStartAsync().Forget();
        }

        private async UniTask ActivateBattleStartAsync()
        {
            var rootRectTm = UIManager.Instance?.CurrPanel?.GetComponent<RectTransform>();

            var battleStartParam = new UI.Popups.BattleStart.Param()
                .WithCompletedAction(OnCompletedBattleStart);

            _battleStart = await UICreator<UI.Popups.BattleStart, UI.Popups.BattleStart.Param>.Get
               .SetRoot(rootRectTm)
               .CreateAsync();
            _battleStart?.Activate(battleStartParam);
        }

        private void OnCompletedBattleStart(TrackEntry trackEntry)
        {
            _battleStart?.Deactivate();

            End();
        }
    }
}

