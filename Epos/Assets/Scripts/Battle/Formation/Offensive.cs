
using System.Collections.Generic;
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

            var iCombatant = _iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);

            Vector3 fromLeaderDir = (iCombatant.Transform.position - LeaderICombatant.Transform.position).normalized;
            Vector2 toTargetDir = (iCombatant.Transform.position - targetPosition).normalized;
            var crossPos = Vector3.Cross(toTargetDir, fromLeaderDir);
            bool isLeft = crossPos.z > 0f;
            Debug.Log(isLeft);
            //var resTargetPosition = targetPosition + (isLeft ? LeaderICombatant.Transform.right * 4f : LeaderICombatant.Transform.right * 4f);

            var traceParam = new Trace.Param()
                .WithTargetTransform(LeaderICombatant?.Transform)
                .WithDistance(0.1f)
                .WithIsLeft(isLeft)
                .WithSpeed(5f);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();

            iCombatant = _iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);

            traceParam = new Trace.Param()
                .WithTargetTransform(LeaderICombatant?.Transform)
                .WithDistance(10f)
                .WithSpeed(5f);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }
    }
}

