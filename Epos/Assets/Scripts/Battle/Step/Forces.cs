using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace Battle.Step
{
    public abstract class Forces : BattleStep<Forces.FieldData>
    {
        public class FieldData : BaseData
        {
            public Parts.Forces Forces = null;
            public bool BattleStart { get; private set; } = true;

            public FieldData SetBattleState(bool battleStart)
            {
                BattleStart = battleStart;

                return this;
            }
        }
        
        public override void Begin()
        {
            BeginAsync().Forget();
        }

        private async UniTask BeginAsync()
        {
            if(_data == null)
                return;
            
            if(_data.BattleStart)
                _data.Forces?.Activate();
            else 
                _data.Forces?.Deactivate();

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            End();
        }    
    }
}

