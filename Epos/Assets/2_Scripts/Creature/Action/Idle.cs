using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace Creature.Action
{
    public class Idle : Act<Idle.Data>
    {
        public class Data : BaseData
        {
            
        }
        
        public override void Execute()
        {
            if (_data == null)
                return;
            
            SetAnimation(_data.AnimationKey, true);
        }
    }
}

