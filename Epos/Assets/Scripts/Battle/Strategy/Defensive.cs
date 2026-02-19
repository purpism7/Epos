using Common;
using Creature;
using Creature.Action;
using GameSystem;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Battle.Strategy
{
    public class Defensive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            base.Apply(iStrategyDataProvider);

            LeaderICombatant = iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
        }

        protected override void MoveFormationFollowers(FormationCompletion completion)
        {
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            AddFormationFollower(iCombatant, DirectionType.Right, 5f, completion);

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            AddFormationFollower(iCombatant, DirectionType.Back, 5f, completion);
        }
    }
}

