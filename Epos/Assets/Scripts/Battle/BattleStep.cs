using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class BattleStep
    {
        public class BaseData
        {
            
        }
        
        protected BattleStep _chainStep = null;

        public virtual void Initialize(BaseData data)
        {
            
        }
        
        public virtual void Begin()
        {
            
        }

        protected virtual void End()
        {
            Debug.Log("End " + GetType());
            
            _chainStep?.Begin();
        }
        
        public void SetChainStep(BattleStep chainStep)
        {
            _chainStep = chainStep;
        }
    }
    
    public abstract class BattleStep<T> : BattleStep where T : BattleStep.BaseData
    {
        protected T _data = null;

        public override void Initialize(BaseData data)
        {
            base.Initialize(data);
            
            _data = data as T;
        }

        public abstract override void Begin();
    }
}

