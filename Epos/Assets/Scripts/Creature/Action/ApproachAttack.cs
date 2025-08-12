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
            Debug.Log("Approach Execute");
            MoveToAttackAsync(_param?.Attacker, _param?.ICombatantList).Forget();
        }

        private async UniTask MoveToAttackAsync(ICombatant attacker, List<ICombatant> iCombatantList)
        {
            var skill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
                return;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

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
            _endAction?.Invoke(_iActor);
        }
        #endregion
    }
}

