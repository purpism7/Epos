using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Battle
{
    public class BattleType
    {
        public interface IListener
        {
            void End();
        }
        
        private BattleStep _firstStep = null;
        private BattleStep _battleStep = null;
        protected IListener _iListener = null;
        
        public virtual void Begin()
        {
            _firstStep?.Begin();
        }

        public virtual void End()
        {
            _iListener?.End();
        }
        
        protected void AddStep<T>(BattleStep.BaseData data = null) where T : BattleStep, new()
        {
            var step = new T();
            step.Initialize(data);

            // 이전 스텝에 chain step 연결.
            _battleStep?.SetChainStep(step);
            _battleStep = step;
            
            if (_firstStep == null)
            {
                _firstStep = step;
            }
        }
    }
    
    public abstract class BattleType<T> : BattleType where T : BattleType<T>.BaseData
    {
        public class BaseData
        {

        }
        
        private T _data = null;
        
        public virtual void Initialize(T data)
        {
            _data = data;
        }
        
        public void SetIListener(IListener iListener)
        {
            _iListener = iListener;
        }
    }
}

