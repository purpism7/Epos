using System.Collections;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Entities;
using GameSystem;
using UI.Panels;

namespace Battle.Step
{
    public class BattleStart : BattleStep
    {
        public override void Begin()
        {
            BeginAsync().Forget();
        }
        
        private async UniTask BeginAsync()
        {
            var battleForces = UIManager.Instance?.GetPanel<BattleForces, BattleForces.Data>();
            battleForces?.Activate();

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            End();
        }
    }
}

