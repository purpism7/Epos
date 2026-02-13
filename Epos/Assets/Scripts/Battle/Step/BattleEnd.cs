using System;
using System.Collections;
using System.Collections.Generic;
using Creator;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using Entities;
using GameSystem;
using UI.Panels;

namespace Battle.Step
{
    public class BattleEnd : BattleStep<BattleEnd.Param>
    {
        [Inject] private UIManager _uiManager = null;

        public class Param : BattleStepParam
        {
            public Action EndAction = null;
        }

        public override void Begin()
        {
            BeginAsync().Forget();
        }

        private async UniTask BeginAsync()
        {
            _uiManager?.CurrView?.Deactivate();
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            _param?.EndAction?.Invoke();

            End();
        }
    }
}

