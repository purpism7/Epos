using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class BattleType
    {
        public interface IListener
        {
            void End();
        }
        
        private IBattleStep _firstStep = null;
        private IBattleStep _lastStep = null;
        
        protected IListener _iListener = null;
        
        public void Begin()
        {
            _firstStep?.Begin();
        }

        public virtual void End()
        {
            _iListener?.End();
        }
        
        protected void AddStep<T>(BattleStep.BaseData data = null, bool isLast = false) where T : BattleStep, new()
        {
            var step = new T();
            var iBattleStep = step.Initialize(data);

            // 이전 스텝에 chain step 연결.
            _lastStep?.SetChainStep(step);
            _lastStep = step;
            
            if (_firstStep == null)
            {
                _firstStep = step;
            }

            if (isLast)
            {
                iBattleStep?.SetLastStepEndAction(EndLastStep);
            }
        }

        private void EndLastStep()
        {
            Debug.Log("EndLastStep");
        }
    }
    
    public abstract class BattleType<T> : BattleType where T : BattleType<T>.BaseData
    {
        public class BaseData
        {
            public BattleMode BattleMode { get; set; } = null;
        }
        
        protected T _data = null;

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

