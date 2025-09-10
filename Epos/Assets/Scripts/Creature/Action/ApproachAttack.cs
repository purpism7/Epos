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
            var skill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
            {
                _endAction?.Invoke(_iActor);
                return;
            }

            var targetList = attacker.GetTargetList(iCombatantList, skill);
            if (targetList.IsNullOrEmpty())
            {
                _endAction?.Invoke(_iActor);
                return;
            }

            var randomIndex = UnityEngine.Random.Range(0, targetList.Count);
            var target = targetList[randomIndex];
            if (target == null ||
                !target.IActor.IsActivate)
            {
                _endAction?.Invoke(_iActor);
                return;
            }

            var skillRange = skill.Range;
            if (skillRange < 0)
            {
                _endAction?.Invoke(_iActor);
                return;
            }

            var moveParam = new Move.Param
            {
                MoveSpeed = attacker.IActor.IStat.Get(Stat.EType.MoveSpeed),
                FinishAction = () =>
                {
                    FinishMoveToTarget(attacker, skill, targetList);
                },
                IsJumpMove = false,
            }
            .WithTargetICombatant(target)?
            .WithForwardDirection(false)?
            .WithDistance(skillRange)
            .WithUseNavMesh(false);

            attacker.IActor.IActCtr?
                .MoveToTarget(moveParam)?
                .Execute();
        }

        private void FinishMoveToTarget(ICombatant attacker, Ability.ISkill iSkill, List<ICombatant> targetList)
        {
            attacker?.IActor.IActCtr?
                .CastingSkill(this, attacker, iSkill, targetList)?
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
            _endAction?.Invoke(_iActor);
        }
        #endregion
    }
}

