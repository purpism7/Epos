using UnityEngine;

namespace Creature.Action.Weight
{
    public class CastSkill : ActionWeight
    {
        protected override int DefaultWeight => 95;

        public override bool CheckCondition()
        {
            return true;
        }

        public override IWeightedAction Create()
        {
            return new Creature.Action.CastSkill();
        }
    }
}
