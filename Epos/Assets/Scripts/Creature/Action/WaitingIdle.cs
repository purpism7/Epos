using UnityEngine;

namespace Creature.Action
{
    public class WaitingIdle : WeightedAction<WaitingIdle.Param>
    {
        public class Param : ActParam
        {

        }

        protected override int Weight => 0;

        public override void Execute()
        {

        }
    }
}

