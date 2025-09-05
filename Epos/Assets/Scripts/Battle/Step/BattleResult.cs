using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using Creator;
using GameSystem;

namespace Battle.Step
{
    public class BattleResult : BattleStep
    {
        [Inject] private UIManager _uiManager = null;
        [Inject] private UIFactory _uiFactory = null;

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

            var uiCreator = _uiFactory?.Create<UI.Popup.BattleState, UI.Popup.BattleState.Param > ();
            var battleState = await uiCreator
                .SetRoot(_uiManager.CurrViewRectTm)
                .CreateAsync();
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
