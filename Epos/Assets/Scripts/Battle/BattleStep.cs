using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public abstract class BattleStep
    {
        private BattleStep _chainStep = null;
        
        public abstract void Begin();

        protected virtual void End()
        {
            _chainStep?.Begin();
        }
        
        public void SetChainStep(BattleStep chainStep)
        {
            _chainStep = chainStep;
        }
    }
}

