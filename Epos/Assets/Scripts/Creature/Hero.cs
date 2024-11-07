using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;
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
            }

            return string.Empty;
        }
        #endregion
    }
}

