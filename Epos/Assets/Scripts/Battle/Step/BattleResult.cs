using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UI.Popup;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine;
using VContainer;

using Creator;

namespace Battle.Step
{
    public class BattleResult : BattleStep<BattleResult.Param>
    {
        public class Param : BattleStepParam
        {
            public bool IsWin { get; private set; } = false;

            public Param WithIsWin(bool isWin)
            {
                IsWin = isWin;
                return this;
            }
        }

        [Inject] private UIManager _uiManager = null;
        [Inject] private UIFactory _uiFactory = null;


        private UI.Popup.BattleWinPopup _battleWinPopup = null;

        public override void Begin()
        {
            BeginAsync().Forget();
        }

        private async UniTask BeginAsync()
        {
            // UICreator<BattleForces, BattleForces.Data>.Get?
                // .Create()?.Deactivate();
            
            // UIManager.Instance?.CurrPanel?.Deactivate();

            //await UniTask.Yield();
            if(_param.IsWin)
                await ActivateBattleWinAsync();
            else 
                End();
        }

        private async UniTask ActivateBattleWinAsync()
        {
            //var rootRectTm = _uiManager?.CurrViewRectTm;
            var uiCreator = _uiFactory?.Create<UI.Popup.BattleWinPopup, UI.Popup.BattleWinPopup.Param>();

            var battleWinPopupParam = new UI.Popup.BattleWinPopup.Param()
                .WithCompletedAction(OnCompletedBattleWin);

            _battleWinPopup = await uiCreator
               //.SetRoot(rootRectTm)
               .CreateAsync();
            _battleWinPopup?.ActivateAsync(battleWinPopupParam);
        }

        private void OnCompletedBattleWin(TrackEntry trackEntry)
        {
            _battleWinPopup?.Deactivate();

            End();
        }
    }
}
