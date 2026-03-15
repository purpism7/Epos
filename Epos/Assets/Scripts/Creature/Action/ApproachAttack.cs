using System.Collections.Generic;
using System;
using UnityEngine;

using Spine.Unity;
using Cysharp.Threading.Tasks;

using Datas.ScriptableObjects;
using Ability;
using Common;

namespace Creature.Action
{
    public class ApproachAttack : WeightedAction<ApproachAttack.Param>, Casting.IListener
    {
        public class Param : WeightedActionParam
        {
            public ICombatant Attacker { get; private set; } = null;
            public List<ICombatant> ICombatantList { get; private set; } = null;

            public Param WithAttacker(ICombatant attacker)
            {
                Attacker = attacker;
                return this;
            }

            public Param WithICombatantList(List<ICombatant> iCombatantList)
            {
                ICombatantList = iCombatantList;
                return this;
            }
        }

        public override void Execute()
        {
            MoveToAttackAsync(_param?.Attacker, _param?.ICombatantList).Forget();
        }

        private async UniTask MoveToAttackAsync(ICombatant attacker, List<ICombatant> iCombatantList)
        {
            if (!attacker?.Transform)
                return;

            var iSkill = attacker.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (iSkill == null)
            {
                End();
                return;
            }

            var targetList = attacker.GetTargetList(iCombatantList, iSkill);
            if (targetList.IsNullOrEmpty())
            {
                End();
                return;
            }

            var closestTarget = attacker.Transform.FindClosestICombatant(targetList);
            var skillData  = iSkill.SkillData;
            if (skillData == null)
            {
                End();
                return; 
            }
            
            var skillRange = skillData.Range;
            if (skillRange > 0)
            {
                // temp thinking...
                if(!string.IsNullOrEmpty(skillData.DashAnimationName))
                {
                    _actor?.SkeletonAnimation?.PlayAnimation(skillData.DashAnimationName, false,
                        (trackEntry) =>
                        {
                            Vector2 direction = closestTarget?.Actor?.SkeletonAnimation.Skeleton.ScaleX > 0 ? Vector2.right : Vector2.left;
                            var targetPosition = (Vector2)closestTarget.Transform.position + direction * skillRange;

                            _actor.SetWorldPosition(targetPosition);

                            CastingSkill(attacker, iSkill, closestTarget, targetList);
                        }, out _duration);

                    return;
                }

                var moveParam = new Move.Param
                {
                    CancellationTokenSource = _param.CancellationTokenSource,

                    MoveSpeed = attacker.Actor.IStat.Get(Stat.EType.MoveSpeed),
                    FinishAction = () =>
                    {
                        CastingSkill(attacker, iSkill, closestTarget, targetList);
                    },
                    IsJumpMove = false,
                }
                .WithTargetICombatant(closestTarget)?
                .WithDistance(skillRange);

                attacker.Actor.ActController?
                    .MoveTo(moveParam)?
                    .Execute();
            }
            else
                CastingSkill(attacker, iSkill, closestTarget,targetList);
        }

        //private void FinishMoveToTarget(ICombatant attacker, Ability.ISkill iSkill, List<ICombatant> targetList)
        //{
        //    CastingSkill(attacker, iSkill, targetList);
        //}

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

        void Casting.IListener.AfterCasting(ICombatant iCombatant)
        {
            End();
        }
        #endregion
    }
}

