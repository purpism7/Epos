using Common;
using Creature;
using Creature.Action;
using System.Collections.Generic;
using System.Linq;
using Entities;
using GameSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>, Casting.IListener
    {
        public class Data : BaseData
        {
            public Transform[] WayPointTms { get; private set; } = null;

            public Data WithWayPointTm(Transform[] wayPointTms)
            {
                WayPointTms = wayPointTms;
                return this;
            }
        }

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            return this;
        }
        
        public override void Begin()
        {
            Debug.Log("Begin()");
            
            MainManager.Get<ICameraManager>().MoveToTarget(_data.WayPointTms[0].position);

            for (int i = 0; i < _data?.EnemyICombatantList.Count; ++i)
            {
                var enemy = _data?.EnemyICombatantList[i];
                enemy?.SetETeam(ETeam.Enemy);
                enemy?.Activate();

                MoveToAttack(enemy);
            }

            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetETeam(ETeam.Ally);
                ally?.Activate();

                MoveToAttack(ally);
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
        
        private void MoveToAttack(ICombatant attacker)
        {
            var skill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
                return;

            List<ICombatant> iCombatantList = null;
            if(attacker.ETeam == ETeam.Ally)
                iCombatantList = _data?.EnemyICombatantList;
            else if(attacker.ETeam == ETeam.Enemy)
                iCombatantList = _data?.AllyICombatantList;

            var targetList = attacker.GetTargetList(iCombatantList, skill);
            if(targetList.IsNullOrEmpty())
                return;
                
            var randomIndex = Random.Range(0, targetList.Count);
            var target = targetList[randomIndex];
            if(target == null)
                return;
                
            var skillRange = skill.Range;
            if (skillRange <= 0)
                return;
               
            //var targetPos = target.Transform.position;
            //var direction = targetPos.x - attacker.Transform.position.x;

            //targetPos.x = direction <= 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
            ////targetPos.y -= 1f;
            //targetPos.z = 0;

            var offsetPosition = new Vector3(skillRange, 0, 0);

            var moveData = new Move.Data
            {
                MoveSpeed = 10f,
                IsJumpMove = false,
                UseNavMesh = false,
            }.WithTargetTm(target.Transform)
            .WithOffsetPosition(offsetPosition);

            attacker.IActCtr?.MoveToTarget(moveData)?
                .CastingSkill(this, attacker, skill, targetList)?
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
            MoveToAttack(iCombatant);
        }
        #endregion
    }
}
