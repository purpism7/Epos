
using System.Collections.Generic;
using UnityEngine;

using Creature;
using GameSystem;

namespace Battle.Formation
{
    public class Offensive : BaseFormation
    {
        public override void Apply(IFormationDataProvider iFormationDataProvider)
        {
            base.Apply(iFormationDataProvider);
    
            LeaderICombatant = iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);
        }
    }
}

