using UnityEngine;

namespace Creature.Action.Weight
{
    public class WaitingIdle : ActionWeight
    {
        protected override int DefaultWeight => 0;

        public override bool CheckCondition()
        {
            return true;
        }

        public override IWeightedAction Create()
        {
            return new Creature.Action.WaitingIdle();
        }
    }
}
