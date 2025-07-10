using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Common;
using Creature;
using Creature.Action;


namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>, Casting.IListener
    {
        public class Data : BaseData
        {

        }

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            return this;
        }
        
        public override void Begin()
        {
            Debug.Log("Begin()");

            for (int i = 0; i < _data?.EnemyICombatantList.Count; ++i)
            {
                var enemy = _data?.EnemyICombatantList[i];
                enemy?.SetETeam(ETeam.Enemy);
                enemy?.Activate();
            }

            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var attacker = _data?.AllyICombatantList[i];
                attacker?.SetETeam(ETeam.Ally);
                attacker?.Activate();

                MoveToAttack(attacker);
            }
        }

        public override void ChainUpdate()
        {
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                _data?.AllyICombatantList[i]?.IActCtr?.ChainUpdate();
            }

            for (int i = 0; i < _data?.EnemyICombatantList.Count; ++i)
            {
                _data?.EnemyICombatantList[i].IActCtr?.ChainUpdate();
            }
        }

        // private ICombatant FindClosestICombatant(List<ICombatant> iCombatantList, ICombatant refICombatant)
        // {
        //     if (iCombatantList.IsNullOrEmpty())
        //         return null;
        //
        //     ICombatant closestEnemyIComtant = null;
        //     float closestDistance = 99999f;
        //     for (int i = 0; i < iCombatantList.Count; ++i)
        //     {
        //         if (iCombatantList[i] == null)
        //             continue;
        //
        //         var distance = Vector2.Distance(iCombatantList[i].Transform.position, refICombatant.Transform.position);
        //         if (closestEnemyIComtant == null ||
        //             closestDistance > distance)
        //         {
        //             closestEnemyIComtant = iCombatantList[i];
        //             closestDistance = distance;
        //         }
        //     }
        //
        //     return closestEnemyIComtant;
        // }
        private void MoveToAttack(ICombatant attacker)
        {
            var skill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
                return;

            var targetList = attacker.GetTargetList(_data?.EnemyICombatantList, skill);
            if(targetList.IsNullOrEmpty())
                return;
                
            var target = targetList.FirstOrDefault();
            if(target == null)
                return;
                
            var skillRange = skill.Range;
            if (skillRange <= 0)
                return;
               
            var targetPos = target.Transform.position;
            var direction = targetPos.x - attacker.Transform.position.x;

            targetPos.x = direction <= 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
            //targetPos.y -= 1f;
            targetPos.z = 0;

            attacker.IActCtr?.MoveToTarget(10f, targetPos,
                () =>
                {


                }, isJumpMove: false, useNavMesh: false);
            attacker.IActCtr?.CastingSkill(this, attacker, skill, targetList);
            attacker.IActCtr?.Execute();
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
            MoveToAttack(iCombatant);
        }
        #endregion
    }
}
