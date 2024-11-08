using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature.Action;

namespace Creature
{
    public class Hero : Character
    {
        public override void Initialize()
        {
            base.Initialize();

            IActCtr = transform.AddOrGetComponent<ActController>();
            IActCtr?.Initialize(this);
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
        }

        #region Act
        public void MoveToTarget(Vector3 pos)
        {
            IActCtr?.MoveToTarget(pos)?.Execute();
        }
        
        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return "F_Idle";
                case Move: return "F_Run";
                case Casting: return "F_Hit_01";
                case Damage: return "F_Damege";
            }

            return string.Empty;
        }
        #endregion
    }
}

