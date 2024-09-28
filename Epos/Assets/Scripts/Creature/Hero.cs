using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Hero : Character
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
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
        }
        
        public void MoveToTarget(Vector3 pos)
        {
            IActCtr?.MoveToTarget("01_F_Run", pos);
        }
    }
}

