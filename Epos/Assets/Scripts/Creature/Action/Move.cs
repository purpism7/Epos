using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Move : Act
    {
        public override void Execute(IActor iActor)
        {
            SetAnimation(iActor, "01_F_Run", true);
        }
    }
}
