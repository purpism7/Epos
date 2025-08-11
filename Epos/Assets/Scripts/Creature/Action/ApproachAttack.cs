using System;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Datas.ScriptableObjects;
using Common;

namespace Creature.Action
{
    public class ApproachAttack : WeightedAction<ApproachAttack.Param>, Casting.IListener
    {
        public class Param : ActionParam
        {

        }

        protected override int Weight => 100;

        public override bool CheckCondition
        {
            get 
            {
                return base.CheckCondition;
            }
        }
            
        public override void Execute()
        {
           
        }

        private async UniTask MoveToAttackAsync(ICombatant attacker, List<ICombatant> iCombatantList, float delay = 0)
        {
            var skill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
                return;

            await UniTask.Yield();
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            //List<ICombatant> iCombatantList = null;
            //if (attacker.ETeam == ETeam.Ally)
            //    iCombatantList = _currWayPoint?.EnemyICombatantList;
            //else if (attacker.ETeam == ETeam.Enemy)
            //    iCombatantList = _data?.AllyICombatantList;

            var targetList = attacker.GetTargetList(iCombatantList, skill);
            if (targetList.IsNullOrEmpty())
            {
                attacker.IActCtr?.Execute();
                return;
            }

            var randomIndex = UnityEngine.Random.Range(0, targetList.Count);
            var target = targetList[randomIndex];
            if (target == null)
            {
                attacker.IActCtr?.Execute();
                return;
            }

            var skillRange = skill.Range;
            if (skillRange <= 0)
            {
                attacker.IActCtr?.Execute();
                return;
            }

            //var targetPos = target.Transform.position;
            //var direction = targetPos.x - attacker.Transform.position.x;

            //targetPos.x = direction <= 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
            ////targetPos.y -= 1f;
            //targetPos.z = 0;

            var offsetPosition = new Vector3(skillRange, 0, 0);
            var moveParam = new Move.Param
            {
                MoveSpeed = attacker.IStat.Get(Stat.EType.MoveSpeed),
                FinishAction = () =>
                {
                    FinishMoveToTarget(attacker, skill, targetList);
                },
                IsJumpMove = false,
                UseNavMesh = false,
            }.WithTargetICombatant(target)
            .WithOffsetPosition(offsetPosition);

            attacker.IActCtr?
                .MoveToTarget(moveParam)?
                .Execute();
        }

        private void FinishMoveToTarget(ICombatant attacker, Skill skill, List<ICombatant> targetList)
        {
            attacker?.IActCtr?
                .CastingSkill(this, attacker, skill, targetList)
                .Execute();
        }

        #region Casting.IListener
        void Casting.IListener.BeforeCasting()
        {
            
        }

        void Casting.IListener.InUse()
        {
            
        }

        void Casting.IListener.AfterCasting(ICombatant iCombatant)
        {
            _endAction?.Invoke();
        }
        #endregion
    }
}

