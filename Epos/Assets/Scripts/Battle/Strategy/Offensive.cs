
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

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);

            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
            TraceTo(iCombatant, DirectionType.Right, 3f);

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            TraceTo(iCombatant, DirectionType.Back, 7f);
        }
    }
}

