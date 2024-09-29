using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Battle
{
    public abstract class BattleType
    {
        public interface IListener
        {
            void End();
        }

        private IListener _iListener = null;

        private BattleStep _firstStep = null;
        private BattleStep _battleStep = null;
        
        public virtual void Initialize(IListener iListener)
        {
            _iListener = iListener;
        }
        
        public virtual void Begin()
        {
            
        }

        public virtual void End()
        {
            _iListener?.End();
        }

        protected void AddStep<T>(bool isFirst = false) where T : BattleStep, new()
        {
            // if (_stepQueue == null)
            // {
            //     _stepQueue = new();
            //     _stepQueue.Clear();
            // }
            
            var step = new T();
            // step.Initialize();

            // 이전 스텝에 chain step 연결.
            _battleStep?.SetChainStep(step);
            _battleStep = step;
            if (isFirst)
            {
                _firstStep = step;
            }
        }

        protected void BeginStep()
        {
            
        }
    }
}

