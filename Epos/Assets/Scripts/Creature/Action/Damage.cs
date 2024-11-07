using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Damage : Act<Damage.Data>
    {
        public class Data : BaseData
        {
            
        }
        
        public override void Execute()
        {
            if (_data == null)
                return;
            
            SetAnimation(_data.AnimationKey, false);
        }
    }
}
