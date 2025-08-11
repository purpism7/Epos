using UnityEngine;

namespace Creature.Action
{
    public class ApproachAttack : WeightedAction<ApproachAttack.Param>
    {
        public class Param : ActParam
        {

        }

        protected override int Weight => 100;

        public override bool CheckCondition
        {
            get 
            {


                return base.CheckCondition;
            }
        }
            
        public override void Execute()
        {
           
        }
    }
}

