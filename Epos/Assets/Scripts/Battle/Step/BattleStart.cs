using System;
using System.Collections;
using System.Collections.Generic;
using Creator;
using Cysharp.Threading.Tasks;

using Entities;
using GameSystem;
using UI.Panels;
using UI.Popups;
using UnityEngine;

namespace Battle.Step
{
    public class BattleStart : BattleStep<BattleStart.Data>
    {
        public class Data : BaseData
        {
            public Parts.Forces LeftForces = null;
            public Parts.Forces RightForces = null;
        }
        
        public override void Begin()
        {
            BeginAsync().Forget();
        }
        
        private async UniTask BeginAsync()
        {
            var battleForcesData = new BattleForces.Data
            {
                LeftForces = _data?.LeftForces,
                RightForces = _data?.RightForces,
            };
            
            UICreator<BattleForces, BattleForces.Data>.Get?
                .SetData(battleForcesData)
                .Create()?.Activate();
                
            // UIManager.Instance?.GetPanel<BattleForces, BattleForces.Data>(battleForcesData);
            // battleForces?.Activate();

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            var battleState = UICreator<BattleState, BattleState.Data>.Get
                ?.SetRoot(UIManager.Instance?.CurrPanel.GetComponent<RectTransform>()).Create();
            // var battleState = UIManager.Instance?.GetPopup<BattleState, BattleState.Data>();
            if (battleState != null)
            {
                // battleState.Activate();
                
                await battleState.StartAsync();
            }

            End();
        }
    }
}

