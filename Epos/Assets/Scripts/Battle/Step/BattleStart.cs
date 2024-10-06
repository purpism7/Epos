using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Step
{
    public class BattleStart : BattleStep
    {
        public override void Begin()
        {
            Debug.Log("Begin " + GetType());
        }
    }
}

