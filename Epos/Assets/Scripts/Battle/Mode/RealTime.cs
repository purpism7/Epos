using Battle.RealTime;
using Common;
using Creator;
using Creature;
using Creature.Action;
using Cysharp.Threading.Tasks;
using GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UI.Parts;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>, WaypointController.IListener, WeightedActionController.IListener, IWeightedActionRequester
    {
        public class Data : BaseData
        {
            public Waypoint[] Waypoints { get; private set; } = null;

            public Data WithWayPoints(Waypoint[] waypoints)
            {
                Waypoints = waypoints;
                return this;
            }
        }

        [Inject] private ICameraManager _iCameraManager = null;
        
        private IWaypointController _iWaypointCtr = null;
        private ICombatant _closestICombatant = null;
        private IWeightedActionController _iWeightedActionCtr = new WeightedActionController();
        private CancellationTokenSource _weightedActionCTS = null;

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            _iWeightedActionCtr?.Initialize(this);
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

        public override void ChainLateUpdate()
        {
            _iWaypointCtr?.ChainLateUpdate();
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

            _iWaypointCtr?.ChainUpdate(_closestICombatant?.Transform);
        }

        private ICombatant ClosestICombatantToWayPoint()
        {
            var wayPoint = _iWaypointCtr?.Waypoint;
            if (wayPoint == null)
                return null;
            
            float closest = 999f;
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

            if(closestICombatant != null)
                _iCameraManager?.SetTargetTm(closestICombatant.Transform);
            
            return closestICombatant;
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            iCombatant?.CreateHpProgress();
        }

        private async UniTask CheckWaypointActionAsync()
        {
            await UniTask.Yield();
            
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                TransitionToIdle();
                return;
            }

            if (waypoint.AliveMonsterCount <= 0)
                MoveToWaypoint(waypoint);
            else
                BeginCombat(waypoint);
        }

        private void MoveToWaypoint(Waypoint waypoint)
        {
            if(_closestICombatant == null)
                _closestICombatant = ClosestICombatantToWayPoint();

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyICombatant = _data?.AllyICombatantList[i];
                if (allyICombatant == null)
                    continue;

                MoveToTarget(waypoint, allyICombatant);
            }
        }

        private void MoveToTarget(Waypoint waypoint, ICombatant iCombatant)
        {
            if (iCombatant == null)
                return;

            if (_closestICombatant == null)
                return;

            float moveSpeed = _closestICombatant.Id == iCombatant.Id ? 7.01f : 7f;
            var moveParam = new Move.Param
            {
                MoveSpeed = moveSpeed,//allyICombatant.IStat.Get(Stat.EType.MoveSpeed),
                TargetPos = waypoint.Position,
            }.WithForwardDirection(_closestICombatant.Id != iCombatant.Id);

            iCombatant.IActCtr?
                .MoveToTarget(moveParam)?
                .Execute();
        }

        private void BeginCombat(Waypoint waypoint)
        {
            _weightedActionCTS = new();

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyICombatant = _data?.AllyICombatantList[i];

                _iWeightedActionCtr?.Execute(allyICombatant, this);
            }

            var enemyICombatantList = waypoint?.EnemyICombatantList;
            for (int i = 0; i < enemyICombatantList?.Count; ++i)
            {
                var enemyICombatant = enemyICombatantList[i];
                if (enemyICombatant == null)
                    continue;

                enemyICombatant.SetETeam(ETeam.Enemy);
                enemyICombatant.Activate();

                CreateHpProgress(enemyICombatant);

                _iWeightedActionCtr?.Execute(enemyICombatant, this);
            }

            _closestICombatant = null;
        }

        private void TransitionToIdle()
        {
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var iCombatant = _data?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                iCombatant.IActCtr?.Execute();
            }
        }

        private async UniTask PrepareForNextActionAsync(ICombatant iCombatant)
        {
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                iCombatant?.IActCtr?.Execute();
                return;
            }

            if (waypoint.AliveMonsterCount <= 0)
            {
                _weightedActionCTS?.Cancel();
                await UniTask.DelayFrame(60);

                if (_closestICombatant == null)
                    _closestICombatant = ClosestICombatantToWayPoint();

                MoveToTarget(waypoint, iCombatant);
            }
            else
                _iWeightedActionCtr?.Execute(iCombatant, this);
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

        CancellationTokenSource IWeightedActionRequester.CancellationTokenSource
        {
            get
            {
                return _weightedActionCTS;
            }
        }
        #endregion

        #region WaypointController.IListener

        void WaypointController.IListener.Arrived()
        {
            TransitionToIdle();
            CheckWaypointActionAsync().Forget();
        }
        #endregion

        #region WeightedActionController.IListener
        void WeightedActionController.IListener.End(ICombatant iCombatant)
        {
            PrepareForNextActionAsync(iCombatant).Forget();
        }
        #endregion
    }
}
