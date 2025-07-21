using System;
using Common;
using Creature;
using Creature.Action;
using System.Collections.Generic;
using System.Linq;
using Creator;
using Cysharp.Threading.Tasks;
using Entities;
using GameSystem;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;
using Datas.ScriptableObjects;
using UI.Parts;


namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>, Casting.IListener
    {
        public class Data : BaseData
        {
            public WayPoint[] WayPoints { get; private set; } = null;
            //public List<> WayPointTms { get; private set; } = null;

            public Data WithWayPoints(WayPoint[] wayPoints)
            {
                WayPoints = wayPoints;
                return this;
            }
        }

        private Queue<WayPoint> _wayPointQueue = null;
        private WayPoint _currWayPoint = null;

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            if (data != null &&
                !data.WayPoints.IsNullOrEmpty())
            {
                if(_wayPointQueue == null)
                    _wayPointQueue = new();

                _wayPointQueue.Clear();

                foreach (var wayPoint in data.WayPoints)
                {
                    _wayPointQueue?.Enqueue(wayPoint);
                }
            }
   
            return this;
        }
        
        public override void Begin()
        {
            Debug.Log("Begin()");

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetETeam(ETeam.Ally);
                ally?.Activate();

                MoveToAttackAsync(ally).Forget();
            }
            
            StartCombatAtWaypointAsync().Forget();
        }

        public override void ChainUpdate()
        {
            if (_currWayPoint != null)
            {
                if(_currWayPoint.AliveMonsterCount <= 0)
                {
                    _currWayPoint = null;
                    StartCombatAtWaypointAsync().Forget();

                    return;
                }
            }
                
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                _data?.AllyICombatantList[i]?.IActCtr?.ChainUpdate();
            }

            for (int i = 0; i < _currWayPoint?.EnemyICombatantList.Count; ++i)
            {
                _currWayPoint?.EnemyICombatantList[i].IActCtr?.ChainUpdate();
            }
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            var hpProgress = UICreator<HpProgress, HpProgress.Data>.Get?
                .Create();

            var targetPos = iCombatant.Transform.position;
            targetPos.y += iCombatant.Height;

            var data = new HpProgress.Data
            {
                TargetTm = iCombatant.Transform,
                Offset = new Vector2(0, iCombatant.Height),
            }.WithCombatant(iCombatant);

            hpProgress?.Activate(data);
        }

        private async UniTask StartCombatAtWaypointAsync()
        {
            

            if (!_wayPointQueue.TryDequeue(out _currWayPoint))
                return;

            //await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            //await UniTask.DelayFrame(12);

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            MainManager.Get<ICameraManager>().MoveToTarget(_currWayPoint.Position);

            for (int i = 0; i < _currWayPoint?.EnemyICombatantList?.Count; ++i)
            {
                var enemy = _currWayPoint?.EnemyICombatantList[i];
                if (enemy == null)
                    continue;

                enemy.SetETeam(ETeam.Enemy);
                enemy.Activate();

                CreateHpProgress(enemy);

                MoveToAttackAsync(enemy).Forget();
            }
        }

        private async UniTask MoveToAttackAsync(ICombatant attacker, float delay = 0)
        {
            var skill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (skill == null)
                return;

            await UniTask.Yield();
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            List<ICombatant> iCombatantList = null;
            if(attacker.ETeam == ETeam.Ally)
                iCombatantList = _currWayPoint?.EnemyICombatantList;
            else if(attacker.ETeam == ETeam.Enemy)
                iCombatantList = _data?.AllyICombatantList;

            var targetList = attacker.GetTargetList(iCombatantList, skill);
            if(targetList.IsNullOrEmpty())
            {
                attacker.IActCtr?.Execute();
                return;
            }
                
            var randomIndex = Random.Range(0, targetList.Count);
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

            var moveData = new Move.Data
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
                .MoveToTarget(moveData)?
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
            MoveToAttackAsync(iCombatant).Forget();
        }
        #endregion
    }
}
