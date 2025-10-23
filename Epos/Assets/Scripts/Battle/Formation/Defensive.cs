using Creature;
using System.Collections.Generic;
using UnityEngine;

using Creature.Action;
using GameSystem;

namespace Battle.Formation
{
    public class Defensive : BaseFormation
    {
        public override void Apply(IFormationDataProvider iFormationDataProvider)
        {
            base.Apply(iFormationDataProvider);

            LeaderICombatant = iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
            Debug.Log(LeaderICombatant.IActor.Id);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);




            var iCombatant = _iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);

            //Vector3 fromLeaderDir = (iCombatant.Transform.position - LeaderICombatant.Transform.position).normalized;
            //Vector2 toTargetDir = (iCombatant.Transform.position - targetPosition).normalized;
            //var crossPos = Vector3.Cross(toTargetDir, fromLeaderDir);
            //bool isLeft = crossPos.z > 0f;

            //var resTargetPosition = targetPosition + (isLeft ? Vector3.left * 4f : Vector3.right * 4f);

            var traceParam = new Trace.Param()
                .WithTargetTransform(LeaderICombatant?.Transform)
                .WithDistance(1f)
                .WithSpeed(5f);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();

            iCombatant = _iFormationDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);

            traceParam = new Trace.Param()
                .WithTargetTransform(LeaderICombatant?.Transform)
                .WithDistance(2f)
                .WithSpeed(5f);

            iCombatant?.IActor?.IActCtr?
                .TraceTo(traceParam)?
                .Execute();
        }
    }
}

