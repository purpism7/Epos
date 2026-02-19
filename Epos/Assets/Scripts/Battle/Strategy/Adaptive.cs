using System.Collections.Generic;
using Common;
using UnityEngine;

using Creature;
using GameSystem;
using Creature.Action;

namespace Battle.Strategy
{
    public class Adaptive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            base.Apply(iStrategyDataProvider);
    
            LeaderICombatant = iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
        }

        public override void InitializeFormationPosition()
        {
            base.InitializeFormationPosition();
            
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            SetFormationPosition(iCombatant, DirectionType.Back, 6f);
            
            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            SetFormationPosition(iCombatant, DirectionType.Left, 6f);
        }

        protected override void MoveFormationFollowers(FormationCompletion completion)
        {
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            AddFormationFollower(iCombatant, DirectionType.Back, 4f, completion);

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            AddFormationFollower(iCombatant, DirectionType.Left, 4f, completion);
        }
    }
}