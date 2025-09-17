using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

using VContainer;
using Cysharp.Threading.Tasks;

using Common;
using GameSystem;
using Creator;
using Lifetime;
using Creature;
using UI.Parts;
using Battle.RealTime;
using Creature.Action;

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
        [Inject] private WeakTypeMap<IActor> _iActorMap = null;
        
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

            _iWaypointCtr = WaypointController.Create(_iResolver, param);
        }

        public override void Begin()
        {
            Debug.Log("Begin()");

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetETeam(ETeam.Ally);
                ally?.IActor?.Activate();
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

                iCombatant.IActor?.IActCtr?.ChainUpdate();
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

                var distance = Vector3.Distance(iCombatant.IActor.Transform.position, wayPoint.Position);
                if (closestICombatant == null || 
                    Vector3.Distance(iCombatant.IActor.Transform.position, wayPoint.Position) < closest)
                {
                    closest = distance;
                    closestICombatant = iCombatant;
                }
            }

            if(closestICombatant != null)
                _iCameraManager?.SetTargetTm(closestICombatant.IActor.Transform);
            
            return closestICombatant;
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            //if (_iHpProgressList == null)
            //{
            //    _iHpProgressList = new();
            //    _iHpProgressList.Clear();
            //}

            var uiCreator = _uiFactory?.Create<HpProgress, HpProgress.Param>();
            var hpProgress = uiCreator?
                .SetWorldUI(true)?
                .Create();

            if (hpProgress == null)
                return;

            //_iHpProgressList?.Add(hpProgress);

            var targetPos = iCombatant.IActor.Transform.position;
            targetPos.y += iCombatant.IActor.Height;

            var param = new HpProgress.Param
            {
                TargetTm = iCombatant?.IActor?.Transform,
                Offset = new Vector2(0, iCombatant.IActor.Height),
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
            if (_closestICombatant.IActor.Id == iCombatant.IActor.Id)
                moveSpeed += 0.01f;

            var moveParam = new Move.Param
            {
                MoveSpeed = moveSpeed,//allyICombatant.IStat.Get(Stat.EType.MoveSpeed),
                TargetPos = waypoint.Position,
            }
            .WithLeaderTm(_closestICombatant.IActor.Transform)
            .WithForwardDirection(_closestICombatant.IActor.Id != iCombatant.IActor.Id);

            iCombatant.IActor.IActCtr?
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
                enemyICombatant.IActor.Activate();

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

                iCombatant.IActor?.IActCtr?.Execute();
            }
        }

        private async UniTask PrepareForNextActionAsync(ICombatant iCombatant)
        {
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                iCombatant?.IActor?.IActCtr?.Execute();
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

        WeightedActionParam IWeightedActionRequester.GetWeightedActionParam(ICombatant attacker, ETeam eTeam, IWeightedAction iWeightedAction)
        {
            switch (iWeightedAction)
            {
                case WeightedAction<ApproachAttack.Param>:
                    {
                        List<ICombatant> iCombatantList = new();
                        iCombatantList.Clear();
                        iCombatantList.AddRange(_iWaypointCtr?.Waypoint?.EnemyICombatantList);
                        iCombatantList.AddRange(_data?.AllyICombatantList);

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
        void WeightedActionController.IListener.End(IActor iActor)
        {
            if(_iActorMap.TryGet<ICombatant>(iActor, out var iCombatant))
                PrepareForNextActionAsync(iCombatant).Forget();

        }
        #endregion
    }
}
