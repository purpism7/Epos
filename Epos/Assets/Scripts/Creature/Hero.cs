using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;
using Creature.Action;

namespace Creature
{
    public class Hero : Character, IActor
    {
        public override float MoveSpeed
        {
            get
            {
                return 8f;
            }
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            IActCtr = gameObject.AddOrGetComponent<ActController>();
            IActCtr?.Initialize(this);
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
        }
        
        #region Act
        public void MoveToTarget(Vector3 pos)
        {
            IActCtr?.MoveToTarget(pos);
        }
        
        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return "00_F_Idle";
                case Move: return "01_F_Run";
            }

            return string.Empty;
        }
        #endregion
    }
}

