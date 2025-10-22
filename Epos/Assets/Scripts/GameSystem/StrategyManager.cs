using UnityEngine;
using System.Collections.Generic;

using VContainer.Unity;

using Strategy;
using Creature;

namespace GameSystem
{
    public interface IStrategyManager
    {
        void Initialize(List<ICombatant> allyICombatantList);
        void ChainUpdate();

        void ApplyStrategy<T>() where T : BaseStrategy, new();
        void MoveFormation(Vector3 targetPosition);

        ICombatant LeaderICombatant { get; }
    }

    public interface IStrategyDataProvider
    {
        List<ICombatant> AllyICombatantList { get; }
    }
    
    public class StrategyManager : IStrategyManager, IStrategyDataProvider
    {
        private IStrategy _currentStrategy = null;
        
        public List<ICombatant> AllyICombatantList { get; private set; } = null;

        #region IStrategyManager
        void IStrategyManager.Initialize(List<ICombatant> allyICombatantList)
        {
            AllyICombatantList = allyICombatantList;

            ApplyStrategy<Defensive>();
        }

        void IStrategyManager.ChainUpdate()
        {
            _currentStrategy?.ChainUpdate();
        }

        void IStrategyManager.ApplyStrategy<T>()
        {
            ApplyStrategy<T>();
        }

        void IStrategyManager.MoveFormation(Vector3 targetPosition)
        {
            _currentStrategy?.MoveFormation(targetPosition);
        }

        ICombatant IStrategyManager.LeaderICombatant
        {
            get
            {
                return _currentStrategy?.LeaderICombatant;
            }
        }
        #endregion

        private void ApplyStrategy<T>() where T : BaseStrategy, new()
        {
            _currentStrategy = new T();
            _currentStrategy?.Apply(this);
        }
    }
}

