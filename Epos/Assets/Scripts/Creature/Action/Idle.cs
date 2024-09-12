using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Idle : Act<ActData>
    {
        public override void Execute(ActData data)
        {
            base.Execute(data);
            
            SetAnimation("00_F_Idle", true);
        }
    }
}

