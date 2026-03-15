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
using UI.View;

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

        //[Inject] private ICameraManager _iCameraManager = null;
        [Inject] private UIManager _uiManager = null;
        [Inject] private UIFactory _uiFactory = null;
        [Inject] private WeakTypeMap<IActor> _iActorMap = null;
        [Inject] private IStrategyController _strategyController = null;

        private IWaypointController _waypointController = null;
        private IWeightedActionController _iWeightedActionCtr = new WeightedActionController();
        private CancellationTokenSource _weightedActionCTS = null;

        private IBattleMainView _battleMainView = null;

        //private bool _isCombating = false;
        private BattleState _battleState = BattleState.None;

        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);

            _iWeightedActionCtr?.Initialize(this);
            InitializeWaypointController();
            
            _strategyController?.Initialize(this, _waypointController, _data?.AllyICombatantList);
            
            return this;
        }

        private void InitializeWaypointController()
        {
            var param = new WaypointController.Param()
                .WithIListener(this)
                .WithWaypoints(_data?.Waypoints);

            _waypointController = WaypointController.Create(_iResolver, param);
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
                ally?.Actor?.Activate();
            }

            CreateBattleMain();
            ProcessNextBattlePhaseAsync().Forget();
        }

        public override void ChainLateUpdate()
        {
            for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
            {
                var iActor = _data.AllyICombatantList[i]?.Actor;
                if (iActor == null || !iActor.IsActivate)
                    continue;
                iActor.ChainLateUpdate();
            }

            _waypointController?.ChainLateUpdate();
        }

        protected override void End(bool isWin)
        {
            base.End(isWin);
            
            _battleMainView?.Deactivate();

            EventHandler.Remove<HeroEmotionEventData>(OnChangedEmotion);
        }

        private void BattleWin()
        {
            var waypoint = _waypointController?.Waypoint;
            if (waypoint == null)
            {
                for (int i = 0; i < _data?.AllyICombatantList?.Count; ++i)
                {
                    var actor = _data?.AllyICombatantList[i]?.Actor;
                    if (actor == null)
                        continue;

                    if (!actor.IsAlive)
                        continue;

                    actor?.ActController?.Victory();
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
                var allies = _data?.AllyICombatantList;
                if(allies != null)
                {
                    for (int i = 0; i < allies.Count; ++i)
                    {
                        var allyICombatant = allies[i];
                        if (allyICombatant == null)
                            continue;

                        if (allyICombatant.Actor.IsAlive)
                            return true;
                    }
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
            _battleMainView = battleMainView;
            battleMainView?.Activate();
        }

        private void UpdateWaypoint()
        {
            var allyList = _data?.AllyICombatantList;
            if(allyList != null)
            {
                for (int i = 0; i < allyList.Count; ++i)
                {
                    var iCombatant = allyList[i];
                    if (iCombatant == null)
                        continue;

                    iCombatant.Actor?.ChainUpdate();
                }
            }

            _waypointController?.ChainUpdate(_strategyController?.LeaderCombatant);
        }

        private void CreateHpProgress(ICombatant combatant)
        {
            if (combatant == null)
                return;
            
            var uiCreator = _uiFactory?.Create<EnemyHpProgressPart, EnemyHpProgressPart.Param>(_iResolver);
            var enemyHpProgressPart = uiCreator?
                .SetWorldUI(true)?
                .Create();

            if (enemyHpProgressPart == null)
                return;

            var param = new EnemyHpProgressPart.Param
            {
                TargetTm = combatant.Actor.Transform,
                Offset = new Vector2(0, combatant.Actor.Height),
            };
            param.WithCombatant(combatant);

            enemyHpProgressPart.ActivateAsync(param);
        }
        
        private void CreateEmotion(ICombatant iCombatant, EmotionType emotionType)
        {
            if (iCombatant == null)
                return;
            
            var uiCreator = _uiFactory?.Create<EmotionPart, EmotionPart.Param>(_iResolver);
            var emotionPart = uiCreator?
                .SetWorldUI(true)?
                .Create();

            if (emotionPart == null)
                return;
            
            var param = new EmotionPart.Param
            {
                TargetTm = iCombatant.Actor.Transform,
                Offset = new Vector2(3f, iCombatant.Actor.Height - 1f),
            }.WithEmotionType(emotionType);

            emotionPart.ActivateAsync(param);
        }

        /// <summary>
        /// 전투 처음 시작 시 한번, Waypoint 도착 시 호출
        /// </summary>
        /// <returns></returns>
        private async UniTask ProcessNextBattlePhaseAsync()
        {
            await UniTask.Yield();
            
            var waypoint = _waypointController?.Waypoint;
            if (waypoint == null)
            {
                BattleWin();
                return;
            }

            if (!_waypointController.HasAliveMonsters)
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
            //_weightedActionCTS = new();

            var enemyCombatants = waypoint?.EnemyICombatantList;
            if (enemyCombatants != null)
            {
                for (int i = 0; i < enemyCombatants.Count; ++i)
                {
                    var enemyCombatant = enemyCombatants[i];
                    if (enemyCombatant == null)
                        continue;

                    enemyCombatant.SetTeamType(TeamType.Enemy);
                    enemyCombatant.Actor.Activate();

                    CreateHpProgress(enemyCombatant);

                    _iWeightedActionCtr?.Execute(enemyCombatant, this, true);
                }
            }
            
            var allies = _data?.AllyICombatantList;
            if (allies != null)
            {
                for (int i = 0; i < allies.Count; ++i)
                {
                    var allyCombatant = allies[i];
                    
                    allyCombatant?.Actor?.ActController?.ClearActQueue();
                    _iWeightedActionCtr?.Execute(allyCombatant, this);
                }
            }
        }

        private async UniTask PrepareForNextActionAsync(ICombatant combatant)
        {
            var actor = combatant?.Actor;
            if (actor == null)
                return;

            if (!actor.IsAlive)
                return;

            var waypoint = _waypointController?.Waypoint;
            if (waypoint == null)
            {
                actor.ActController?.Execute();
                return;
            }

            if (!_waypointController.HasAliveMonsters)
            {
                if (_battleState == BattleState.Combat)
                    _battleState = BattleState.MoveWayPoint;

                if(combatant == _strategyController.LeaderCombatant)
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
            if (_waypointController == null)
                return null;

            if (_data == null)
                return null;
            
            switch (iWeightedAction)
            {
                case WeightedAction<ApproachAttack.Param>:
                    {
                        List<ICombatant> combatants = new();
                        combatants.Clear();
                        combatants.AddRange(_waypointController.Waypoint.EnemyICombatantList);
                        combatants.AddRange(_data.AllyICombatantList);

                        var param = new ApproachAttack.Param
                        {
                            CancellationTokenSource = LinkedCancellationTokenSource
                        }
                        .WithAttacker(attacker)
                        .WithICombatantList(combatants);

                        return param;
                    }
            }

            return null;
        }

        //CancellationTokenSource IWeightedActionRequester.CancellationTokenSource
        private CancellationTokenSource LinkedCancellationTokenSource
        {
            get
            {
                return CancellationTokenSource.CreateLinkedTokenSource(_strategyController.CancellationToken);
            }
        }
        #endregion

        #region WaypointController.IListener

        void WaypointController.IListener.Arrived()
        {
            // TransitionToIdle();
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
        void StrategyController.IListener.OnChangedStrategy(IStrategy strategy)
        {
            _battleState = BattleState.Formation;
            _battleMainView?.OnChangedStrategy(strategy);
        }

        void StrategyController.IListener.OnEndRegroupToLeader(IStrategy strategy)
        {
            OnEndRegroupToLeaderAsync(strategy).Forget();
        }

        private async UniTask OnEndRegroupToLeaderAsync(IStrategy strategy)
        {
            var waypoint = _waypointController?.Waypoint;
            if (waypoint == null)
            {
                BattleWin();
                return;
            }

            // await UniTask.Delay(500);
            
            if (!_waypointController.HasAliveMonsters)
                _battleState = BattleState.MoveWayPoint;
            else
                _battleState = BattleState.Combat;

            // Trace 종료 후 ActController의 ExecuteAsync가 완료될 시간을 주어, Casting이 누락되는 타이밍 이슈 방지
            var allies = _data?.AllyICombatantList;
            if (allies != null)
            {
                for (int i = 0; i < allies.Count; ++i)
                {
                    var allyCombatant = allies[i];
                    var actController = allyCombatant?.Actor?.ActController;
                    if (actController == null)
                        continue;

                    actController.ClearActQueue();
                    actController.Execute();

                    await PrepareForNextActionAsync(allyCombatant);
                }
            }
        }
        #endregion

        #region Event

        private void OnChangedEmotion(HeroEmotionEventData eventData)
        {
            if (eventData == null)
                return;

            var combatant = _data?.AllyICombatantList?.Find(combatant => combatant.Actor.Id == eventData.CharacterId);
            if (combatant == null)
                return;

            CreateEmotion(combatant, eventData.EmotionType);
        }
        #endregion
    }
}
