
using UnityEngine;
using System.Collections.Generic;

using Common;
using Creature;
using GameSystem;
using Creature.Action;

namespace Battle.Strategy
{
    public class Offensive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            base.Apply(iStrategyDataProvider);
    
            LeaderICombatant = iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
        }

        protected override void MoveFormationFollowers(FormationCompletion completion)
        {
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
            AddFormationFollower(iCombatant, DirectionType.Right, 3f, completion);

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            AddFormationFollower(iCombatant, DirectionType.Back, 7f, completion);
        }
    }
}

