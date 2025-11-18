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
using Battle.Strategy;

namespace Battle.Mode
{
    public class RealTime : BattleMode<RealTime.Data>, WaypointController.IListener, WeightedActionController.IListener, IWeightedActionRequester, StrategyController.IListener
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
        [Inject] private IStrategyController _iStrategyController = null;

        private IWaypointController _iWaypointCtr = null;
        private IWeightedActionController _iWeightedActionCtr = new WeightedActionController();
        private CancellationTokenSource _weightedActionCTS = null;

        private bool _isCombating = false;

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            _iWeightedActionCtr?.Initialize(this);
            InitializeWaypointController();
            
            _iStrategyController?.Initialize(this, _data?.AllyICombatantList);
            
            return this;
        }

        private void InitializeWaypointController()
        {
            var param = new WaypointController.Param()
                .WithIListener(this)
                .WithWaypoints(_data?.Waypoints);

            _iWaypointCtr = WaypointController.Create(_iResolver, param);
        }

        public override void ChainUpdate()
        {
            _iStrategyController?.ChainUpdate();
            UpdateWaypoint();

            //if (_closestICombatant?.IActor != null)
            //{
            //    if (!_closestICombatant.IActor.IsAlive)
            //        SetClosestICombatant();
            //}
        }

        public override void Begin()
        {
            Debug.Log("Begin()");
            
            
            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetETeam(ETeam.Ally);
                ally?.IActor?.Activate();

                CreateEmotion(ally);
            }

            ActivateBattleMain();
            CheckWaypointActionAsync().Forget();
        }

        public override void ChainLateUpdate()
        {
            _iWaypointCtr?.ChainLateUpdate();
        }

        private void BattleWin()
        {
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
                {
                    var allyIActor = _data?.AllyICombatantList[i]?.IActor;
                    if (allyIActor == null)
                        continue;

                    if (!allyIActor.IsAlive)
                        continue;

                    allyIActor?.IActCtr?.Victory();
                }
            }

            End(true);
        }

        private void BattleLose()
        {
            End(false);
        }

        private bool IsAllyAlive
        {
            get
            {
                for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
                {
                    var allyICombatant = _data?.AllyICombatantList[i];
                    if (allyICombatant == null)
                        continue;

                    if (allyICombatant.IActor.IsAlive)
                        return true ;
                }

                return false;
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
            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var iCombatant = _data?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                iCombatant.IActor?.ChainUpdate();
            }

            _iWaypointCtr?.ChainUpdate(_iStrategyController?.LeaderICombatant);
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            var uiCreator = _uiFactory?.Create<EnemyHpProgressPart, EnemyHpProgressPart.Param>();
            var enemyHpProgressPart = uiCreator?
                .SetWorldUI(true)?
                .Create();

            if (enemyHpProgressPart == null)
                return;

            var param = new EnemyHpProgressPart.Param
            {
                TargetTm = iCombatant?.IActor?.Transform,
                Offset = new Vector2(0, iCombatant.IActor.Height),
            };
            param.WithCombatant(iCombatant);

            enemyHpProgressPart?.ActivateAsync(param);
        }
        
        private void CreateEmotion(ICombatant iCombatant)
        {
            var uiCreator = _uiFactory?.Create<EmotionPart, EmotionPart.Param>();
            var emotionPart = uiCreator?
                .SetWorldUI(true)?
                .Create();

            if (emotionPart == null)
                return;
            
            var param = new EmotionPart.Param
            {
                TargetTm = iCombatant?.IActor?.Transform,
                Offset = new Vector2(0, iCombatant.IActor.Height),
            };

            emotionPart?.ActivateAsync(param);
        }

        private async UniTask CheckWaypointActionAsync()
        {
            await UniTask.Yield();
            
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                BattleWin();
                return;
            }

            if (waypoint.AliveMonsterCount <= 0)
                MoveToWaypoint(waypoint);
            else
                BeginCombat(waypoint);
        }

        private void MoveToWaypoint(Waypoint waypoint)
        {
            _iStrategyController?.MoveFormation(waypoint.Position);
        }

        private void BeginCombat(Waypoint waypoint)
        {
            if (_isCombating)
                return;

            _isCombating = true;
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
            var iActor = iCombatant?.IActor;
            if (iActor == null)
                return;

            if (!iActor.IsAlive)
                return;

            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                iCombatant.IActor?.IActCtr?.Execute();
                return;
            }

            if (waypoint.AliveMonsterCount <= 0)
            {
                if (_isCombating)
                    _isCombating = false;

                _weightedActionCTS?.Cancel();
                _weightedActionCTS = null;

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                if(iCombatant == _iStrategyController.LeaderICombatant)
                    MoveToWaypoint(waypoint);
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
            if (_iActorMap.TryGet<ICombatant>(iActor, out var iCombatant))
                PrepareForNextActionAsync(iCombatant).Forget();

            if(!IsAllyAlive)
                BattleLose();

            //if (_closestICombatant != null &&
            //    _closestICombatant.IActor != null)
            //{
            //    if(!_closestICombatant.IActor.IsAlive)
            //    {
            //        _closestICombatant = null;
            //        _closestICombatant = ClosestICombatantToWayPoint();
            //    }
            //}
        }
        #endregion

        #region StrategyController.IListener
        void StrategyController.IListener.OnChangedStrategy(IStrategy iStrategy, bool isInitalized)
        {
            _iCameraManager?.SetTargetTm(iStrategy?.LeaderICombatant?.Transform);
            
            if(!isInitalized)
                CheckWaypointActionAsync().Forget();
        }
        #endregion
    }
}
