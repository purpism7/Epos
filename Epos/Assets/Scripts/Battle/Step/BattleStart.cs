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
    public class BattleStart : BattleStep<BattleStart.Param>
    {
        public class Param : BattleStepParam
        {
            public Datas.ScriptableObjects.Party AllyParty { get; private set; } = null;
            public Datas.ScriptableObjects.Party EnemyParty { get; private set; } = null;

            public Param(Datas.ScriptableObjects.Party allyParty, Datas.ScriptableObjects.Party enemyParty)
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
            //if(_param?.AllyParty != null &&
            //   _param?.EnemyParty != null)
            //{
            //    var param = new BattleForces.Param(_param?.AllyParty, _param?.EnemyParty);

            //    UICreator<BattleForces, BattleForces.Param>.Get?
            //        .SetParam(param)
            //        .Create()?
            //        .Activate(param);
            //}

            //await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            var rootRectTm = UIManager.Instance?.CurrPanel?.GetComponent<RectTransform>();

            var battleStart = await UICreator<UI.Popups.BattleStart, UI.Popups.BattleStart.Param>.Get
                .SetRoot(rootRectTm)
                .CreateAsync();
            battleStart?.Activate();

            if (battleStart != null)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(4f));
                battleStart.Deactivate();               
            }

            var battleMainViewParam = new UI.View.BattleMainView.Param()
                .WithAllyParty(_param?.AllyParty);

            //await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            var battleMainView = UICreator<UI.View.BattleMainView, UI.View.BattleMainView.Param>.Get
                .SetParam(battleMainViewParam)
                .SetRoot(rootRectTm)
                .Create();
            battleMainView?.Activate();


            End();
        }
    }
}

