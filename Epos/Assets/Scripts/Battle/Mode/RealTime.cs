using Battle.RealTime;
using Common;
using Creator;
using Creature;
using Creature.Action;
using Cysharp.Threading.Tasks;
using GameSystem;
using Lifetime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UI.Parts;
using UnityEditor;
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
        [Inject] private UIManager _uiManager = null;
        [Inject] private UIFactory _uiFactory = null;
        
        private IWaypointController _iWaypointCtr = null;
        private ICombatant _closestICombatant = null;
        private IWeightedActionController _iWeightedActionCtr = new WeightedActionController();
        private CancellationTokenSource _weightedActionCTS = null;
        private List<IHpProgress> _iHpProgressList = null;

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

            _iWaypointCtr = WaypointController.Create(_iResolver, param);
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

            ActivateBattleMain();

            CheckWaypointActionAsync().Forget();
        }

        public override void ChainUpdate()
        {
            UpdateWaypoint();
        }

        public override void ChainLateUpdate()
        {
            _iWaypointCtr?.ChainLateUpdate();
            LateUpdateHpProgress();
        }

        private void LateUpdateHpProgress()
        {
            for (int i = 0; i < _iHpProgressList?.Count; ++i)
            {
                _iHpProgressList[i]?.ChainLateUpdate();
            }
        }
        
        private void ActivateBattleMain()
        {
            var rootRectTm = _uiManager?.CurrViewRectTm;
            var uiCreator = _uiFactory?.Create<UI.View.BattleMainView, UI.View.BattleMainView.Param>();

            var battleMainViewParam = new UI.View.BattleMainView.Param()
               .WithAllyICombatantList(_data?.AllyICombatantList);

            var battleMainView = uiCreator?
               .SetParam(battleMainViewParam)
               .SetRoot(rootRectTm)
               .Create();
            battleMainView?.Activate();
        }

        private void UpdateWaypoint()
        {
            for (int i = 0; i < _data?.AllyICombatantList.Count; ++i)
            {
                var iCombatant = _data?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                iCombatant.IActCtr?.ChainUpdate();
            }

            _iWaypointCtr?.ChainUpdate(_closestICombatant);
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
            if (_iHpProgressList == null)
            {
                _iHpProgressList = new();
                _iHpProgressList.Clear();
            }

            var uiCreator = _uiFactory?.Create<HpProgress, HpProgress.Param>();
            var hpProgress = uiCreator?.SetWorldUI(true)?
                .Create();

            if (hpProgress == null)
                return;

            _iHpProgressList?.Add(hpProgress);

            var targetPos = iCombatant.Transform.position;
            targetPos.y += iCombatant.Height;

            var param = new HpProgress.Param
            {
                TargetTm = iCombatant.Transform,
                Offset = new Vector2(0, iCombatant.Height),
            }.WithCombatant(iCombatant);

            hpProgress?.ActivateAsync(param);
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

                MoveToWaypoint(waypoint, allyICombatant);
            }
        }

        private void MoveToWaypoint(Waypoint waypoint, ICombatant iCombatant)
        {
            if (iCombatant == null)
                return;

            if (_closestICombatant == null)
                return;

            float moveSpeed = 7f;
            if (_closestICombatant.Id == iCombatant.Id)
                moveSpeed += 0.01f;

            var moveParam = new Move.Param
            {
                MoveSpeed = moveSpeed,//allyICombatant.IStat.Get(Stat.EType.MoveSpeed),
                TargetPos = waypoint.Position,
            }
            .WithLeaderTm(_closestICombatant.Transform)
            .WithForwardDirection(_closestICombatant.Id != iCombatant.Id);

            iCombatant.IActCtr?
                .MoveToTarget(moveParam)?
                .Execute();
        }

        private void BeginCombat(Waypoint waypoint)
        {
            _weightedActionCTS = new();

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

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyICombatant = _data?.AllyICombatantList[i];

                _iWeightedActionCtr?.Execute(allyICombatant, this);
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

                if (_closestICombatant == null)
                    _closestICombatant = ClosestICombatantToWayPoint();

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                //Debug.Log("movetoWaypoint = " + iCombatant.Id);
                MoveToWaypoint(waypoint, iCombatant);
            }
            else
                _iWeightedActionCtr?.Execute(iCombatant, this);
        }

        #region RealTime.IProvider

        WeightedActionParam IWeightedActionRequester.GetWeightedActionParam(ICombatant attacker, IWeightedAction iWeightedAction)
        {
            switch (iWeightedAction)
            {
                case WeightedAction<ApproachAttack.Param>:
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
