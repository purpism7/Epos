using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Idle : Act<Idle.Data>
    {
        public class Data : ActData
        {
            public string Key = string.Empty;
        }
        
        public override void Execute(Data data)
        {
            base.Execute(data);
            
            SetAnimation(data.Key, true);
        }
    }
}

