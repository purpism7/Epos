using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

using Datas.ScriptableObjects;
using Spine.Unity;

using Ability;
using Common;

namespace Creature.Action
{
    public class ApproachAttack : WeightedAction<ApproachAttack.Param>, Casting.IListener
    {
        public class Param : WeightedActionParam
        {
            public ICombatant Attacker { get; private set; } = null;
            public List<ICombatant> Combatants { get; private set; } = null;

            public Param WithAttacker(ICombatant attacker)
            {
                Attacker = attacker;
                return this;
            }

            public Param WithICombatantList(List<ICombatant> combatants)
            {
                Combatants = combatants;
                return this;
            }
        }

        public override void Execute()
        {
            MoveToAttack(_param?.Attacker, _param?.Combatants);
        }

        protected override void End()
        {
            base.End();
        }

        private void MoveToAttack(ICombatant attacker, List<ICombatant> iCombatantList)
        {
            var actController = attacker?.ActController;

            if (!attacker?.Transform)
            {
                End();
                return;
            }

            var skill = attacker.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
            {
                End();
                return;
            }

            var targetList = attacker.GetTargetList(iCombatantList, skill);
            if (targetList.IsNullOrEmpty())
            {
                End();
                return;
            }

            var closestTarget = attacker.Transform.FindClosestICombatant(targetList);
            var skillData  = skill.SkillData;
            if (skillData == null)
            {
                End();
                return;
            }

            var skillRange = skillData.Range;
            if (skillRange > 0)
            {
                if (TryDashAndCastSkill(attacker, skill, closestTarget, targetList))
                    return;

                MoveTo(attacker, skill, skillRange, closestTarget, targetList);
            }
            else
                CastingSkill(attacker, skill, closestTarget,targetList);
        }

        private bool TryDashAndCastSkill(ICombatant attacker, 
            Ability.ISkill skill,
            ICombatant target, 
            List<ICombatant> targets)
        {
            var skillData = skill.SkillData;
            if (skillData == null)
                return false;

            if (string.IsNullOrEmpty(skillData.DashAnimationName))
                return false;

            attacker.ActController.SetBusy(true);

            _actor?.SkeletonAnimation?.PlayAnimation(skillData.DashAnimationName, false,
                (trackEntry) =>
                {
                    Vector2 direction = target?.Actor?.SkeletonAnimation.Skeleton.ScaleX > 0 ? Vector2.right : Vector2.left;
                    var targetPosition = (Vector2)target.Transform.position + direction * skillData.Range;

                    _actor.SetWorldPosition(targetPosition);

                    attacker.ActController.SetBusy(false);

                    CastingSkill(attacker, skill, target, targets);
                }, out _duration);

            return true;
        }

        private void MoveTo(ICombatant attacker,
            Ability.ISkill skill,
            float skillRange,
            ICombatant target,
            List<ICombatant> targets)
        {
            var moveParam = new Move.Param
            {
                CancellationTokenSource = _param.CancellationTokenSource,

                MoveSpeed = attacker.Actor.IStat.Get(Stat.EType.MoveSpeed),
                FinishAction = () =>
                {
                    CastingSkill(attacker, skill, target, targets);
                },
                IsJumpMove = false,
            }
               .WithTargetICombatant(target)?
               .WithDistance(skillRange);

            attacker.Actor.ActController?
                .MoveTo(moveParam)?
                .Execute();
        }


        private void CastingSkill (ICombatant attacker, Ability.ISkill skill, ICombatant target, List<ICombatant> targets)
        {
            var actor = attacker?.Actor;
            if (actor == null ||
                !actor.IsAlive)
            {
                End();
                return;
            }

            actor.ActController?
                .CastingSkill(this, attacker, skill, target, targets)?
                .Execute();
        }

        #region Casting.IListener
        void Casting.IListener.BeforeCasting()
        {
            
        }

        void Casting.IListener.InUse()
        {
            
        }

        void Casting.IListener.AfterCasting(ICombatant combatant)
        {
            End();
        }
        #endregion
    }
}

