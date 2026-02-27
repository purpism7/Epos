using System.Collections.Generic;
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
            void OnChangedStrategy();
            void OnEndRegroupToLeader(IStrategy iStrategy);
        }

        [Inject] private ICameraManager _cameraManager = null;

        private IListener _listener = null;

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

        private void ApplyStrategy(IStrategy strategy, bool isInitalized = false)
        {
            strategy?.Apply(this);
            CurrentIStrategy = strategy;

            _listener?.OnChangedStrategy();

            RegroupToLeaderAsync(strategy, isInitalized).Forget();
        }

        private async UniTask RegroupToLeaderAsync(IStrategy strategy, bool isInitalized)
        {
            if (strategy == null)
                return;

            _cameraManager?.SetTargetTr(strategy.LeaderICombatant?.Transform, Vector3.zero);

            if(!isInitalized)
            {
                await strategy.RegroupToLeaderAsync();

                _listener?.OnEndRegroupToLeader(strategy);
            } 
        }
    }
}
