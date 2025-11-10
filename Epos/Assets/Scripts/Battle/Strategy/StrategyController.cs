using System.Collections.Generic;
using UnityEngine;

using Creature;
using GameSystem;

namespace Battle.Strategy
{
    public interface IStrategyController
    {
        void Initialize(List<ICombatant> allyICombatantList);
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
        public List<ICombatant> AllyICombatantList { get; private set; } = null;

        public IStrategy CurrentIStrategy { get; private set; } = null;

        #region IStrategyController
        void IStrategyController.Initialize(List<ICombatant> allyICombatantList)
        {
            AllyICombatantList = allyICombatantList;

            ApplyStrategy(new Adaptive());
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

        private void ApplyStrategy(IStrategy iStrategy)
        {
            iStrategy?.Apply(this);
            CurrentIStrategy = iStrategy;
        }
    }

}
