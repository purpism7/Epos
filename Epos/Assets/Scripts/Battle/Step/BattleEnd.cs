using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Entities;
using GameSystem;
using UI.Panels;

namespace Battle.Step
{
    public class BattleEnd : BattleStep<BattleEnd.Data>
    {
        public class Data : BaseData
        {
            public Action EndAction = null;
        }

        public override void Begin()
        {
            BeginAsync().Forget();
        }

        private async UniTask BeginAsync()
        {
            var battleForces = UIManager.Instance?.GetPanel<BattleForces, BattleForces.Data>();
            battleForces?.Deactivate();

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            MainManager.Get<IFieldManager>()?.Activate();
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            _data?.EndAction?.Invoke();

            End();
        }
    }
}

