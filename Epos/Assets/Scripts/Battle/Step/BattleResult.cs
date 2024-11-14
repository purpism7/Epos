using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using GameSystem;
using UI.Panels;

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
            var battleState = UIManager.Instance?.GetPanel<BattleState, BattleState.Data>();
            if (battleState != null)
            {
                battleState.Activate();
                
                await battleState.WinAsync();
            }
            
            End();
        }
    }
}
