using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Parts;

namespace UI.Panels
{
    public class BattleForces : Panel<BattleForces.Data>
    {
        public class Data : Base
        {
            public Forces LeftForces = null;
            public Forces RightForces = null;
        }

        public override Panel<Data> Initialize(Data data)
        {
            base.Initialize(data);

            return this;
        }
    }
}

