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

        protected Queue<BattleStep> _stepQueue = null;
        
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

        protected void AddStep<T>() where T : BattleStep, new()
        {
            if (_stepQueue == null)
            {
                _stepQueue = new();
                _stepQueue.Clear();
            }
            
            var step = new T();
            // step.Initialize();
           
            _stepQueue?.Enqueue(step);
        }

        protected void BeginStep()
        {
            
        }
    }
}

