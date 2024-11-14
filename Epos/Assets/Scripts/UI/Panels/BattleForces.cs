using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Panels
{
    public class BattleForces : Panel<BattleForces.Data>
    {
        public class Data : Base
        {
            
        }

        public override Panel<Data> Initialize(Data data)
        {
            base.Initialize(data);

            return this;
        }
    }
}

