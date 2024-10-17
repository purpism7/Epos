using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Idle : Act<Idle.Data>
    {
        public class Data : BaseData
        {
            
        }
        
        public override void Execute(Data data)
        {
            base.Execute(data);

            var iActorTm = data?.IActor?.Transform;
            if (!iActorTm)
                return;
            
            SetAnimation(data.Key, true);
        }
    }
}

