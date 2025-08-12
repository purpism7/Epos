using UnityEngine;

namespace Creature.Action.Weight
{
    public class ApproachAttack : ActionWeight
    {
        protected override int DefaultWeight => 100;

        public override bool CheckCondition()
        {
            return true;
        }

        public override IWeightedAction Create()
        {
            return new Creature.Action.ApproachAttack();
        }
    }
}
