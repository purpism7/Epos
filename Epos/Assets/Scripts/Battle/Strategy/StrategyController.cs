using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

using VContainer;

using Creature;
using Datas.ScriptableObjects;
using GameSystem;

namespace Battle.Strategy
{
    public interface IStrategyController
    {
        void Initialize(StrategyController.IListener iListener, List<ICombatant> allyICombatantList);
        void ChainUpdate();

        void ApplyStrategy(IStrategy iStrategy);
        void MoveFormation(Vector3 targetPosition);

        ICombatant LeaderICombatant { get; }

        IStrategy CurrentIStrategy { get; }
    }

    public interface IStrategyDataProvider
    {
        List<ICombatant> AllyICombatantList { get; }
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
        private CancellationTokenSource _regroupCancelTokenSource = null;

        public List<ICombatant> AllyICombatantList { get; private set; } = null;
        public IStrategy CurrentIStrategy { get; private set; } = null;

        #region IStrategyController
        void IStrategyController.Initialize(IListener iListener, List<ICombatant> allyICombatantList)
        {
            _listener = iListener;
            AllyICombatantList = allyICombatantList;
            
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

        ICombatant IStrategyController.LeaderICombatant
        {
            get
            {
                return CurrentIStrategy?.LeaderICombatant;
            }
        }
        #endregion

        private void ApplyStrategy(IStrategy strategy, bool isInitialized = false)
        {
            strategy?.Apply(this);
            CurrentIStrategy = strategy;

            _listener?.OnChangedStrategy(strategy);

            // Cancel을 먼저 호출하고, Dispose는 그 후에 합니다.
            // 이미 Dispose된 경우를 대비해 Try-Catch로 감싸거나 null 체크를 정교하게 합니다.
            if (_regroupCancelTokenSource != null)
            {
                try 
                {
                    _regroupCancelTokenSource?.Cancel();
                }
                catch (ObjectDisposedException) 
                {
                    // 이미 Dispose되었다면 무시합니다.
                }
                finally 
                {
                
                    _regroupCancelTokenSource?.Dispose();
                    _regroupCancelTokenSource = null;
                }
            }
            
            RegroupToLeaderAsync(strategy, isInitialized).Forget();
        }

        private async UniTask RegroupToLeaderAsync(IStrategy strategy, bool isInitialized)
        {
            if (strategy == null)
                return;

            _cameraManager?.SetTargetTr(strategy.LeaderICombatant?.Transform, Vector3.zero);

            if(!isInitialized)
            {
                if(_regroupCancelTokenSource == null)
                    _regroupCancelTokenSource = new CancellationTokenSource();
                
                await strategy.RegroupToLeaderAsync(_regroupCancelTokenSource.Token);
                
                _listener?.OnEndRegroupToLeader(strategy);
            } 
        }
    }
}
