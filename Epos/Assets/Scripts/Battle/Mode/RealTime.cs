using System.Collections.Generic;
using UnityEngine;

using Common;
using Creature;
using Creature.Action;
using GameSystem.Event;
using System.Reflection;


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
                enemy?.Activate();
            }

            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var attacker = _data?.AllyICombatantList[i];
                attacker?.Activate();

                var target = FindClosestICombatant(_data?.EnemyICombatantList, attacker);
                if(target != null)
                {
                    var skill = attacker.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
                    if (skill == null)
                        continue;

                    var skillRange = skill.Range;
                    if (skillRange <= 0)
                        continue;

                    var targetPos = target.Transform.position;
                    var direction = targetPos.x - attacker.Transform.position.x;

                    targetPos.x = direction <= 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
                    //targetPos.y -= 1f;
                    targetPos.z = 0;

                    attacker?.IActCtr?.MoveToTarget(2f, targetPos,
                        () =>
                        {


                        }, isJumpMove: false, useNavMesh: false);
                    attacker?.IActCtr.CastingSkill(this, attacker, skill, new List<ICombatant>() { target });
                    attacker?.IActCtr?.Execute();
                }
               
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

        private ICombatant FindClosestICombatant(List<ICombatant> iCombatantList, ICombatant refICombatant)
        {
            if (iCombatantList.IsNullOrEmpty())
                return null;

            ICombatant closestEnemyIComtant = null;
            float closestDistance = 99999f;
            for (int i = 0; i < iCombatantList.Count; ++i)
            {
                if (iCombatantList[i] == null)
                    continue;

                var distance = Vector2.Distance(iCombatantList[i].Transform.position, refICombatant.Transform.position);
                if (closestEnemyIComtant == null ||
                    closestDistance > distance)
                {
                    closestEnemyIComtant = iCombatantList[i];
                    closestDistance = distance;
                }
            }

            return closestEnemyIComtant;
        }

        #region Casting.IListener
        void Casting.IListener.BeforeCasting()
        {

        }

        void Casting.IListener.InUse()
        {

        }

        void Casting.IListener.AfterCasting()
        {

        }
        #endregion
    }
}
