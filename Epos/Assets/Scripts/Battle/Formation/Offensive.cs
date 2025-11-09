
using System.Collections.Generic;
using Common;
using UnityEngine;

using Creature;
using GameSystem;
using Creature.Action;

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

            // var debugGameObject = new GameObject("DebugGameObject"); 
            // var debugObject = debugGameObject.AddComponent<DebugObject>();
            // debugObject.originTm = LeaderICombatant.Transform; 
            // debugObject.targetPosition = targetPosition; 
            
            var moveSpeed = MainManager.Instance.TraceMoveSpeed;
            
            var iCombatant = _iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
            
            var traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Left)
                .WithDistance(5f)
                .WithSpeed(moveSpeed);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();

            iCombatant = _iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);

            traceParam = new Trace.Param()
                .WithTargetICombatant(LeaderICombatant)
                .WithDirectionType(DirectionType.Back)
                .WithDistance(10f)
                .WithSpeed(moveSpeed);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }
    }
}

