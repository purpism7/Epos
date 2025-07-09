using System.Collections.Generic;
using UnityEngine;

using Common;
using Creature;
using Creature.Action;
using GameSystem.Event;


namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>
    {
        public class Data : BaseData
        {

        }

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            return this;
        }
        
        public override void Begin()
        {
            Debug.Log("Begin()");   
            
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var iActCtr = _data?.AllyICombatantList[i]?.IActCtr;
                // iActCtr.Move
            }
        }

        public override void ChainUpdate()
        {
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                _data?.AllyICombatantList[i]?.IActCtr?.ChainUpdate();
            }
        }
    }
}
