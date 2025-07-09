using Common;
using Creature.Action;
using GameSystem.Event;
using UnityEngine;


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

        /// <summary>
        /// 전투 시작.
        /// </summary>
        public override void Begin()
        {
            
        }

        public override void ChainUpdate()
        {
            
        }
    }
}
