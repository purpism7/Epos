using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Creature;

namespace Battle.Step
{
    public abstract class Party : BattleStep<Party.Param>
    {
        public class Param : BattleStepParam
        {
            public Parts.PartyLocation PartyLocation = null;
            public Datas.ScriptableObjects.Party Party { get; private set; } = null;
            public List<ICombatant> ICombatantList { get; private set; } = null;

            public bool BattleStart { get; private set; } = true;

            public Param SetBattleState(bool battleStart)
            {
                BattleStart = battleStart;

                return this;
            }

            public Param WithParty(Datas.ScriptableObjects.Party party)
            {
                Party = party;
                return this;
            }

            public Param WithICombatantList(List<ICombatant> iCombatantList)
            {
                ICombatantList = iCombatantList;
                return this;
            }
        }
        
        public override void Begin()
        {
            BeginAsync().Forget();
        }

        private async UniTask BeginAsync()
        {
            if(_param == null)
                return;
            
            if(_param.BattleStart)
                _param.PartyLocation?.Activate();
            else
                _param.PartyLocation?.Deactivate();

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            End();
        }    
    }
}

