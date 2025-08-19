using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using VContainer;
using Cysharp.Threading.Tasks;

using Battle.RealTime;
using Common;
using Creator;
using Creature;
using Creature.Action;
using GameSystem;
using UI.Parts;
using Unity.VisualScripting;

namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>, WaypointController.IListener, WeightedActionExecutor.IListener, IWeightedActionRequester
    {
        public class Data : BaseData
        {
            public Waypoint[] Waypoints { get; private set; } = null;
            //public List<> WayPointTms { get; private set; } = null;

            public Data WithWayPoints(Waypoint[] waypoints)
            {
                Waypoints = waypoints;
                return this;
            }
        }

        [Inject] private ICameraManager _iCameraManager = null;
        
        private IWaypointController _iWaypointCtr = null;
        private ICombatant _closestICombatant = null;
            

        //private HashSet<>
        private IWeightedActionExecutor _iWeightedActionExecutor = new WeightedActionExecutor();

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            _iWeightedActionExecutor?.Initialize(this);

            InitializeWaypointController();
            
            return this;
        }

        private void InitializeWaypointController()
        {
            var param = new WaypointController.Param()
                .WithIListener(this)
                .WithWaypoints(_data?.Waypoints);

            _iWaypointCtr = WaypointController.Create(param);
        }
        
        public override void Begin()
        {
            Debug.Log("Begin()");

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetETeam(ETeam.Ally);
                ally?.Activate();
            }
            
            CheckWaypointActionAsync().Forget();
        }

        public override void ChainUpdate()
        {
            UpdateWaypoint();
        }

        private void UpdateWaypoint()
        {
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var iCombatant = _data?.AllyICombatantList[i];
                if(iCombatant == null)
                    continue;

                iCombatant.IActCtr?.ChainUpdate();
            }
            
            if(_closestICombatant != null)
                _iWaypointCtr?.ChainUpdate(_closestICombatant?.Transform);
        }

        private ICombatant ClosestICombatantToWayPoint()
        {
            var wayPoint = _iWaypointCtr?.Waypoint;
            if (wayPoint == null)
                return null;
            
            float closest = 99f;
            ICombatant closestICombatant = null;
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var iCombatant = _data?.AllyICombatantList[i];
                if(iCombatant == null)
                    continue;

                var distance = Vector3.Distance(iCombatant.Transform.position, wayPoint.Position);
                if (closestICombatant == null || 
                    Vector3.Distance(iCombatant.Transform.position, wayPoint.Position) < closest)
                {
                    closest = distance;
                    closestICombatant = iCombatant;
                }
            }

            return closestICombatant;
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            var hpProgress = UICreator<HpProgress, HpProgress.Param>.Get?
                .Create();

            var targetPos = iCombatant.Transform.position;
            targetPos.y += iCombatant.Height;

            var param = new HpProgress.Param
            {
                TargetTm = iCombatant.Transform,
                Offset = new Vector2(0, iCombatant.Height + 0.5f),
            }.WithCombatant(iCombatant);

            hpProgress?.Activate(param);
        }

        private async UniTask CheckWaypointActionAsync()
        {
            await UniTask.Yield();
            
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
                return;

            if (waypoint.AliveMonsterCount <= 0)
                MoveToWaypoint(waypoint);
            else
                BeginCombat(waypoint);
        }

        private void MoveToWaypoint(Waypoint waypoint)
        {
            _closestICombatant = ClosestICombatantToWayPoint();
            if (_closestICombatant == null)
                return;

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyICombatant = _data?.AllyICombatantList[i];
                if (allyICombatant == null)
                    continue;

                var moveParam = new Move.Param
                {
                    MoveSpeed = 5f,//allyICombatant.IStat.Get(Stat.EType.MoveSpeed),
                    TargetPos = waypoint.Position,
                }.WithForwardDirection(_closestICombatant.Id != allyICombatant.Id);

                allyICombatant.IActCtr?
                    .MoveToTarget(moveParam)
                    .Execute();
            }
        }

        private void BeginCombat(Waypoint waypoint)
        {
            var enemyICombatantList = waypoint?.EnemyICombatantList;

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyICombatant = _data?.AllyICombatantList[i];

                _iWeightedActionExecutor?.Execute(allyICombatant, this);
            }

            for (int i = 0; i < enemyICombatantList?.Count; ++i)
            {
                var enemyICombatant = enemyICombatantList[i];
                if (enemyICombatant == null)
                    continue;

                enemyICombatant.SetETeam(ETeam.Enemy);
                enemyICombatant.Activate();

                CreateHpProgress(enemyICombatant);

                _iWeightedActionExecutor?.Execute(enemyICombatant, this);
            }
        }

        #region RealTime.IProvider

        WeightedActionParam IWeightedActionRequester.GetWeightedActionParam(ICombatant attacker, IWeightedAction iWeightedAction)
        {
            switch (iWeightedAction)
            {
                case WeightedAction<ApproachAttack.Param> approachAttack:
                    {
                        List<ICombatant> iCombatantList = null;
                        if (attacker.ETeam == ETeam.Ally)
                            iCombatantList = _iWaypointCtr?.Waypoint?.EnemyICombatantList;
                        else if (attacker.ETeam == ETeam.Enemy)
                            iCombatantList = _data?.AllyICombatantList;

                        var param = new ApproachAttack.Param()
                            .WithAttacker(attacker)
                            .WithICombatantList(iCombatantList);

                        return param;
                    }
            }

            return null;
        }

        #endregion

        #region WaypointController.IListener

        void WaypointController.IListener.Arrived()
        {
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var iCombatant = _data?.AllyICombatantList[i];
                if(iCombatant == null)
                    continue;
            
                iCombatant.IActCtr?.Execute();
            }
            
            // CheckWaypointActionAsync().Forget();
        }
        #endregion

        #region WeightedActionExecutor.IListener
        void WeightedActionExecutor.IListener.End(ICombatant iCombatant)
        {
            _iWeightedActionExecutor?.Execute(iCombatant, this);

            if (_iWaypointCtr?.Waypoint.AliveMonsterCount <= 0)
            {
                MoveToWaypoint(_iWaypointCtr?.Waypoint);
            }
        }
        #endregion
    }
}
