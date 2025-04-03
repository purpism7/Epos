using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Creator;
using GameSystem;
using UI.Popups;

namespace Battle.Step
{
    public class BattleResult : BattleStep
    {
        public override void Begin()
        {
            BeginAsync().Forget();
        }

        private async UniTask BeginAsync()
        {
            // UICreator<BattleForces, BattleForces.Data>.Get?
                // .Create()?.Deactivate();
            
            // UIManager.Instance?.CurrPanel?.Deactivate();

            await UniTask.Yield();


            var battleState = UICreator<BattleState, BattleState.Data>.Get
                ?.SetRoot(UIManager.Instance?.CurrPanel.GetComponent<RectTransform>()).Create();
            // var battleState = UIManager.Instance?.Get<BattleState, BattleState.Data>();
            if (battleState != null)
            {
                battleState.Activate();
                
                await battleState.WinAsync();
            }
            
            End();
        }
    }
}
