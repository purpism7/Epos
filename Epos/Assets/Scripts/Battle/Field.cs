using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class Field : BattleType
    {
        public override void Initialize(IListener iListener)
        {
            base.Initialize(iListener);

            AddStep<Deploy>();
        }

        public override void Begin()
        {
            base.Begin();
        }
    }
}

