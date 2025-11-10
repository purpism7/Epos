using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;
using Creature.Action;
using GameSystem;

namespace Battle.Strategy
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
        protected IStrategyDataProvider _iStrategyDataProvider = null;
        protected float _moveSpped = 1f;

        public ICombatant LeaderICombatant { get; protected set; } = null;

        public virtual void Apply(IStrategyDataProvider iStrategyDataProvider)
        { 
            _iStrategyDataProvider = iStrategyDataProvider;
        }

        public virtual void ChainUpdate()
        {
            
        }

        public virtual void MoveFormation(Vector3 targetPosition)
        {
            _moveSpped = LeaderICombatant.IStat.Get(Stat.EType.MoveSpeed);

           var moveParam = new Move.Param
            {
                MoveSpeed = _moveSpped,
                TargetPos = targetPosition,
            }.WithTargetICombatant(null);

            LeaderICombatant?.IActor?.IActCtr?
                .MoveToTarget(moveParam)?
                .Execute();

            var debugObjGmeObj = new GameObject();
            var debugObj = debugObjGmeObj.AddComponent<DebugObject>();
            debugObj.originTm = LeaderICombatant.Transform;
            debugObj.targetPosition = targetPosition;
        }
    }
}
