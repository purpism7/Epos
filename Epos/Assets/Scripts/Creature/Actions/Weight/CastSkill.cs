using UnityEngine;

namespace Creature.Actions.Weight
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
            return new Creature.Actions.CastSkill();
        }
    }
}
