using UnityEngine;

namespace Creature.Actions
{
    public class CastSkill : WeightedAction<CastSkill.Param>
    {
        public class Param : WeightedActionParam
        {

        }

        protected override int Weight => 95;

        public override void Execute()
        {

        }
    }
}

