using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

using VContainer;

using Creature;
using Datas.ScriptableObjects;
using GameSystem;
using Battle.RealTime;

namespace Battle.Strategy
{
    public interface IStrategyController
    {
        void Initialize(StrategyController.IListener iListener, IWaypointController waypointController, List<ICombatant> allyICombatantList);
        void ChainUpdate();

        void ApplyStrategy(IStrategy iStrategy);
        void MoveFormation(Vector3 targetPosition);

        ICombatant LeaderCombatant { get; }

        IStrategy CurrentIStrategy { get; }
        CancellationToken CancellationToken { get; }
    }

    public interface IStrategyDataProvider
    {
        List<ICombatant> Allycombatants { get; }
    }

    public class StrategyController : IStrategyController, IStrategyDataProvider
    {
        public interface IListener
        {
            void OnChangedStrategy(IStrategy strategy);
            void OnEndRegroupToLeader(IStrategy strategy);
        }

        [Inject] private ICameraManager _cameraManager = null;

        private IListener _listener = null;
        private IWaypointController _waypointController = null;
        private CancellationTokenSource _cancellationTokenSource = null;

        public List<ICombatant> Allycombatants { get; private set; } = null;
        public IStrategy CurrentIStrategy { get; private set; } = null;
        public CancellationToken CancellationToken
        {
            get
            {
                if (_cancellationTokenSource == null)
                    _cancellationTokenSource = new();

                return _cancellationTokenSource.Token;
            }
        }

        #region IStrategyController
        void IStrategyController.Initialize(IListener iListener, IWaypointController waypointController, List<ICombatant> allyICombatantList)
        {
            _listener = iListener;
            _waypointController = waypointController;
            Allycombatants = allyICombatantList;
            
            ApplyStrategy(new Adaptive(), true);
            CurrentIStrategy?.InitializeFormationPosition();
        }

        void IStrategyController.ChainUpdate()
        {
            CurrentIStrategy?.ChainUpdate();
        }

        void IStrategyController.ApplyStrategy(IStrategy strategy)
        {
            ApplyStrategy(strategy);
        }

        void IStrategyController.MoveFormation(Vector3 targetPosition)
        {
            CurrentIStrategy?.MoveFormation(targetPosition);
        }

        ICombatant IStrategyController.LeaderCombatant
        {
            get
            {
                return CurrentIStrategy?.LeaderCombatant;
            }
        }
        #endregion

        private void ApplyStrategy(IStrategy strategy, bool isInitialized = false)
        {
            if (_cancellationTokenSource != null)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // 이미 Dispose된 경우 무시
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            // 3. 이전 전략의 Trace 콜백 제거
            CurrentIStrategy?.CleanupTraceCallbacks();
            if (Allycombatants != null)
            {
                for (int i = 0; i < Allycombatants.Count; i++)
                {
                    Allycombatants[i]?.Actor?.ActController?.ClearActQueue();
                }
            }

            // 5. 새 전략 적용
            strategy?.Apply(this);
            CurrentIStrategy = strategy;

            _listener?.OnChangedStrategy(strategy);

            RegroupToLeaderAsync(strategy, isInitialized, _cancellationTokenSource.Token).Forget();
        }

        private async UniTask RegroupToLeaderAsync(IStrategy strategy, bool isInitialized, CancellationToken cancellationToken)
        {
            if (strategy == null)
                return;

            _cameraManager?.SetTargetTr(strategy.LeaderCombatant?.Transform, Vector3.zero);

            if (!isInitialized)
            {
                Vector3? targetPosition = null;
                var waypoint = _waypointController?.Waypoint;
                if (waypoint != null)
                {
                    if (!waypoint.HasAliveMonsters)
                        targetPosition = waypoint.Position;
                }

                await strategy.RegroupToLeaderAsync(targetPosition, cancellationToken);

                // 취소된 경우 OnEndRegroupToLeader 호출하지 않음
                if (!cancellationToken.IsCancellationRequested)
                {
                    _listener?.OnEndRegroupToLeader(strategy);
                }
            }
        }
    }
}
