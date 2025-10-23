using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;
using Creature.Action;
using GameSystem;

namespace Battle.Formation
{
    public interface IFormation
    {
        void Apply(IFormationDataProvider iFormationDataProvider);

        void ChainUpdate();

        
        void MoveFormation(Vector3 targetPosition);

        ICombatant LeaderICombatant { get; }
}

    public abstract class BaseFormation : IFormation
    {
        private const float LeaderMoveSpeed = 5f;

        protected IFormationDataProvider _iFormationDataProvider = null;

        public ICombatant LeaderICombatant { get; protected set; } = null;

        public virtual void Apply(IFormationDataProvider iFormationDataProvider)
        {
            _iFormationDataProvider = iFormationDataProvider;
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
