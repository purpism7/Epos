using UnityEngine;

namespace Creature.Action
{
    public class CastSkill : WeightedAction<CastSkill.Param>
    {
        public class Param : ActParam
        {

        }

        protected override int Weight => 95;

        public override void Execute()
        {

        }
    }
}

