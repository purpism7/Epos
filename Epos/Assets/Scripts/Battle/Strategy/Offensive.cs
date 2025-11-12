
using System.Collections.Generic;
using Common;
using UnityEngine;

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

            // var debugGameObject = new GameObject("DebugGameObject"); 
            // var debugObject = debugGameObject.AddComponent<DebugObject>();
            // debugObject.originTm = LeaderICombatant.Transform; 
            // debugObject.targetPosition = targetPosition; 

            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
            
            var traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Right)
                .WithDistance(3f)
                .WithSpeed(_moveSpped);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();

            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);

            traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Back)
                .WithDistance(7f)
                .WithSpeed(_moveSpped);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }
    }
}

