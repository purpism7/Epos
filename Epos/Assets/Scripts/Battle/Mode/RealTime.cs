using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

using VContainer;

using Battle.RealTime;
using Battle.Strategy;
using Common;
using Creator;
using Creature;
using Creature.Action;
using GameSystem;
using GameSystem.Event;
using Lifetime;
using System.Linq;
using System.Threading;
using UI.Parts;
using UI.Popup;

namespace Battle.Mode
{
    public enum BattleState
    {
        None,

        Combat,
        MoveWayPoint,
        Formation,
    }

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
        [Inject] private IStrategyController _strategyController = null;

        private IWaypointController _iWaypointCtr = null;
        private IWeightedActionController _iWeightedActionCtr = new WeightedActionController();
        private CancellationTokenSource _weightedActionCTS = null;

        //private bool _isCombating = false;
        private BattleState _battleState = BattleState.None;

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            _iWeightedActionCtr?.Initialize(this);
            InitializeWaypointController();
            
            _strategyController?.Initialize(this, _data?.AllyICombatantList);
            
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
            _strategyController?.ChainUpdate();
            UpdateWaypoint();
        }

        public override void Begin()
        {
            Debug.Log("Begin()");
            EventHandler.Add<HeroEmotionEventData>(OnChangedEmotion);
            
            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var ally = _data?.AllyICombatantList[i];
                ally?.SetTeamType(TeamType.Ally);
                ally?.IActor?.Activate();
            }

            CreateBattleMain();
            ProcessNextBattlePhaseAsync().Forget();
        }

        public override void ChainLateUpdate()
        {
            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var iActor = _data.AllyICombatantList[i]?.IActor;
                if (iActor == null || !iActor.IsActivate)
                    continue;
                iActor.ChainLateUpdate();
            }
            _iWaypointCtr?.ChainLateUpdate();
        }

        protected override void End(bool isWin)
        {
            base.End(isWin);
            
            EventHandler.Remove<HeroEmotionEventData>(OnChangedEmotion);
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
        
        private void CreateBattleMain()
        {
            var rootRectTm = _uiManager?.CurrViewRectTm;
            var uiCreator = _uiFactory?.Create<UI.View.BattleMainView, UI.View.BattleMainView.Param>(_iResolver);

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

            _iWaypointCtr?.ChainUpdate(_strategyController?.LeaderICombatant);
        }

        private void CreateHpProgress(ICombatant iCombatant)
        {
            var uiCreator = _uiFactory?.Create<EnemyHpProgressPart, EnemyHpProgressPart.Param>(_iResolver);
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
        
        private void CreateEmotion(ICombatant iCombatant, EmotionType emotionType)
        {
            var uiCreator = _uiFactory?.Create<EmotionPart, EmotionPart.Param>(_iResolver);
            var emotionPart = uiCreator?
                .SetWorldUI(true)?
                .Create();

            if (emotionPart == null)
                return;
            
            var param = new EmotionPart.Param
            {
                TargetTm = iCombatant?.IActor?.Transform,
                Offset = new Vector2(3f, iCombatant.IActor.Height - 1f),
            }.WithEmotionType(emotionType);

            emotionPart?.ActivateAsync(param);
        }

        private async UniTask ProcessNextBattlePhaseAsync()
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
            _strategyController?.MoveFormation(waypoint.Position);
        }

        private void BeginCombat(Waypoint waypoint)
        {
            if (_battleState == BattleState.Combat)
                return;

            _battleState = BattleState.Combat;
            _weightedActionCTS = new();

            var enemyCombatantList = waypoint?.EnemyICombatantList;
            for (int i = 0; i < enemyCombatantList?.Count; ++i)
            {
                var enemyCombatant = enemyCombatantList[i];
                if (enemyCombatant == null)
                    continue;

                enemyCombatant.SetTeamType(TeamType.Enemy);
                enemyCombatant.IActor.Activate();

                CreateHpProgress(enemyCombatant);

                _iWeightedActionCtr?.Execute(enemyCombatant, this, true);
            }

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyCombatant = _data?.AllyICombatantList[i];
                _iWeightedActionCtr?.Execute(allyCombatant, this);
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

        private async UniTask PrepareForNextActionAsync(ICombatant combatant)
        {
            var actor = combatant?.IActor;
            if (actor == null)
                return;

            if (!actor.IsAlive)
                return;

            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                actor.IActCtr?.Execute();
                return;
            }

            if (waypoint.AliveMonsterCount <= 0)
            {
                if (_battleState == BattleState.Combat)
                    _battleState = BattleState.MoveWayPoint;

                _weightedActionCTS?.Cancel();
                _weightedActionCTS = null;

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                if(combatant == _strategyController.LeaderICombatant)
                    MoveToWaypoint(waypoint);
            }
            else
            {
                _iWeightedActionCtr?.Execute(combatant, this);
            }
        }

        #region RealTime.IProvider

        WeightedActionParam IWeightedActionRequester.GetWeightedActionParam(ICombatant attacker, TeamType teamType, IWeightedAction iWeightedAction)
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
            ProcessNextBattlePhaseAsync().Forget();
        }
        #endregion

        #region WeightedActionController.IListener
        void WeightedActionController.IListener.End(IActor actor)
        {
            if (!IsAllyAlive)
            {
                BattleLose();
                return;
            }
                
            if (_iActorMap.TryGet<ICombatant>(actor, out var combatant))
            {
                if(combatant.TeamType == TeamType.Ally)
                {
                    if (_battleState == BattleState.Formation)
                        return;
                }

                PrepareForNextActionAsync(combatant).Forget();
            }
        }
        #endregion

        #region StrategyController.IListener
        void StrategyController.IListener.OnChangedStrategy()
        {
            _battleState = BattleState.Formation;
        }

        void StrategyController.IListener.OnEndRegroupToLeader(IStrategy strategy)
        {
            var waypoint = _iWaypointCtr?.Waypoint;
            if (waypoint == null)
            {
                BattleWin();
                return;
            }

            if (waypoint.AliveMonsterCount <= 0)
                _battleState = BattleState.MoveWayPoint;
            else
                _battleState = BattleState.Combat;

            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var allyCombatant = _data?.AllyICombatantList[i];
                PrepareForNextActionAsync(allyCombatant).Forget();
            }
        }
        #endregion

        #region Event

        private void OnChangedEmotion(HeroEmotionEventData eventData)
        {
            if (eventData == null)
                return;

            var combatant = _data?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == eventData.CharacterId);
            if (combatant == null)
                return;

            CreateEmotion(combatant, eventData.EmotionType);
        }
        #endregion
    }
}
