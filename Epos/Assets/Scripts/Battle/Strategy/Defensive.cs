using UnityEngine;
using Creature;
using System.Collections.Generic;

using Creature.Action;
using Common;
using GameSystem;

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

            var traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Right)
                .WithDistance(5f)
                .WithSpeed(_moveSpped);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);

            traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Back)
                .WithDistance(5f)
                .WithSpeed(_moveSpped);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }
    }
}

