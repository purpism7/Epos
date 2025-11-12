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

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);
            
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            
            var traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Back)
                .WithDistance(4f)
                .WithSpeed(_moveSpped);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);

            traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Right)
                .WithDistance(4f)
                .WithSpeed(_moveSpped);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }
    }
}