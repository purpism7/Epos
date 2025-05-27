using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace Battle.Step
{
    public abstract class Party : BattleStep<Party.FieldData>
    {
        public class FieldData : BaseData
        {
            public Parts.PartyLocation PartyLocation = null;
            public Datas.ScriptableObjects.Party Party { get; private set; } = null;
            public bool BattleStart { get; private set; } = true;

            public FieldData SetBattleState(bool battleStart)
            {
                BattleStart = battleStart;

                return this;
            }

            public FieldData WithParty(Datas.ScriptableObjects.Party party)
            {
                Party = party;
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
                _data.PartyLocation?.Activate();
            else 
                _data.PartyLocation?.Deactivate();

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            End();
        }    
    }
}

