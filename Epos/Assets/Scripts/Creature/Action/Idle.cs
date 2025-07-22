using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace Creature.Action
{
    public class Idle : Act<Idle.Param>
    {
        public class Param : ActParam
        {
            
        }
        
        public override void Execute()
        {
            if (_param == null)
                return;
            
            SetAnimation(_param.AnimationKey, true);
        }
    }
}

