using System.Collections.Generic;
using UnityEngine;

using VContainer;

using Creature;
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
            void OnChangedStrategy(IStrategy iStrategy, bool isInitalized = false);
        }

        private IListener _iListener = null;

        public List<ICombatant> AllyICombatantList { get; private set; } = null;
        public IStrategy CurrentIStrategy { get; private set; } = null;

        #region IStrategyController
        void IStrategyController.Initialize(IListener iListener, List<ICombatant> allyICombatantList)
        {
            _iListener = iListener;
            AllyICombatantList = allyICombatantList;
            
            ApplyStrategy(new Adaptive(), true);
            CurrentIStrategy?.InitializeFormationPosition();
        }

        void IStrategyController.ChainUpdate()
        {
            CurrentIStrategy?.ChainUpdate();
        }

        void IStrategyController.ApplyStrategy(IStrategy iStrategy)
        {
            ApplyStrategy(iStrategy);
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

        private void ApplyStrategy(IStrategy iStrategy, bool isInitalized = false)
        {
            iStrategy?.Apply(this);
            CurrentIStrategy = iStrategy;

            _iListener?.OnChangedStrategy(iStrategy, isInitalized);
        }
    }
}
