using Creature;
using Creature.Action;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Strategy
{
    public interface IStrategy
    {
        void Apply(IStrategyDataProvider iStrategyDataProvider);

        void ChainUpdate();

        
        void MoveFormation(Vector3 targetPosition);

        ICombatant LeaderICombatant { get; }
}

    public abstract class BaseStrategy : IStrategy
    {
        private const float LeaderMoveSpeed = 5f;

        protected IStrategyDataProvider _iStrategyDataProvider = null;

        public ICombatant LeaderICombatant { get; protected set; } = null;

        public virtual void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            _iStrategyDataProvider = iStrategyDataProvider;
        }

        public virtual void ChainUpdate()
        {
            // Default implementation (can be overridden by derived classes)
        }

        public virtual void MoveFormation(Vector3 targetPosition)
        {
            var moveParam = new Move.Param
            {
                MoveSpeed = LeaderMoveSpeed,//allyICombatant.IStat.Get(Stat.EType.MoveSpeed),
                TargetPos = targetPosition,
            }.WithTargetICombatant(null);

            LeaderICombatant?.IActor?.IActCtr?
                .MoveToTarget(moveParam)?
                .Execute();
        }
    }
}
