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

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            // temp
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            TraceTo(iCombatant, DirectionType.Right, 5f);

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            TraceTo(iCombatant, DirectionType.Back, 5f);
        }
    }
}

