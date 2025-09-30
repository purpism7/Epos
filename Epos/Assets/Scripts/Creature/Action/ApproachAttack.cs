using System;
using System.Collections.Generic;
using UnityEngine;

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
            var iSkill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
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
                var moveParam = new Move.Param
                {
                    MoveSpeed = attacker.IActor.IStat.Get(Stat.EType.MoveSpeed),
                    FinishAction = () =>
                    {
                        CastingSkill(attacker, iSkill, closestTarget, targetList);
                    },
                    IsJumpMove = false,
                }
                .WithTargetICombatant(closestTarget)?
                .WithForwardDirection(false)?
                .WithDistance(skillRange)
                .WithUseNavMesh(false);

                attacker.IActor.IActCtr?
                    .MoveToTarget(moveParam)?
                    .Execute();
            }
            else
                CastingSkill(attacker, iSkill, closestTarget,targetList);
        }

        //private void FinishMoveToTarget(ICombatant attacker, Ability.ISkill iSkill, List<ICombatant> targetList)
        //{
        //    CastingSkill(attacker, iSkill, targetList);
        //}

        private void CastingSkill (ICombatant attacker, Ability.ISkill iSkill, ICombatant target, List<ICombatant> targetList)
        {
            if(target == null ||
              !target.IActor.IsAlive)
            {
                End();
                return;
            }    

            attacker?.IActor?.IActCtr?
                .CastingSkill(this, attacker, iSkill, target, targetList)?
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

