using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Step
{
    public class BattleResult : BattleStep
    {
        public override void Begin()
        {
            End();
        }
    }
}
