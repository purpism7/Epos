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

namespace Battle.Step
{
    public class BattleStart : BattleStep<BattleStart.Data>
    {
        public class Data : BaseData
        {
            public Datas.ScriptableObjects.Party AllyParty { get; private set; } = null;
            public Datas.ScriptableObjects.Party EnemyParty { get; private set; } = null;

            public Data(Datas.ScriptableObjects.Party allyParty, Datas.ScriptableObjects.Party enemyParty)
            {
                AllyParty = allyParty;
                EnemyParty = enemyParty;
            }
        }
        
        public override void Begin()
        {
            BeginAsync().Forget();
        }
        
        private async UniTask BeginAsync()
        {
            var battleForcesData = new BattleForces.Data(_data?.AllyParty, _data?.EnemyParty);
            
            UICreator<BattleForces, BattleForces.Data>.Get?
                .SetData(battleForcesData)
                .Create()?
                .Activate(battleForcesData);

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            var battleStart = UICreator<UI.Popups.BattleStart, UI.Popups.BattleStart.Data>.Get
                ?.SetRoot(UIManager.Instance?.CurrPanel.GetComponent<RectTransform>()).Create();
            
            // var battleState = UIManager.Instance?.GetPopup<BattleState, BattleState.Data>();
            if (battleStart != null)
            {
                // battleState.Activate();

                await UniTask.Delay(TimeSpan.FromSeconds(4f));
                battleStart.Deactivate();
            }

            End();
        }
    }
}

