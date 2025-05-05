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
            public Parts.PartyLocation Left = null;
            public Parts.PartyLocation Right = null;
        }
        
        public override void Begin()
        {
            BeginAsync().Forget();
        }
        
        private async UniTask BeginAsync()
        {
            var battleForcesData = new BattleForces.Data
            {
                Left = _data?.Left,
                Right = _data?.Right,
            };
            
            UICreator<BattleForces, BattleForces.Data>.Get?
                .SetData(battleForcesData)
                .Create()?.Activate();
                
            // UIManager.Instance?.GetPanel<BattleForces, BattleForces.Data>(battleForcesData);
            // battleForces?.Activate();

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

