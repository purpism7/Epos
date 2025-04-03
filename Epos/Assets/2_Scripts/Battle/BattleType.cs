using System.Collections;
using System.Collections.Generic;
using Battle.Step;
using UnityEngine;

namespace Battle
{
    public class BattleType : BattleMode.IListener
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
            _firstStep = null;
        }

        protected virtual void End()
        {
            
        }

        public virtual void ChainUpdate()
        {
            
        }
        
        protected void AddStep<T>(BattleStep.BaseData data = null, bool isLast = false) where T : BattleStep, new()
        {
            var step = new T();
            var iBattleStep = step.Initialize(data);

            // 이전 스텝에 chain step 연결.
            _lastStep?.SetChainStep(step);
            _lastStep = step;
            
            if (_firstStep == null)
                _firstStep = step;

            if (isLast)
                iBattleStep?.SetLastStepEndAction(EndLastStep);
        }

        private void EndLastStep()
        {
            if (_lastStep is BattleStart)
                Ready();
                
            _lastStep = null;
        }
        
        protected virtual void Ready()
        {
            
        }

        protected void BattleEnd()
        {
            _iListener?.End();
        }
        
        #region BattleMode.IListener
        void BattleMode.IListener.End()
        {
            End();
        }
        #endregion
    }
    
    public abstract class BattleType<T> : BattleType where T : BattleType<T>.BaseData
    {
        public class BaseData
        {
            public BattleMode BattleMode = null;
        }
        
        protected T _data = null;

        public virtual void Initialize(T data)
        {
            _data = data;
            
            _data?.BattleMode?.SetIListener(this);
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
            
            _data?.BattleMode?.ChainUpdate();
        }
        
        public void SetIListener(IListener iListener)
        {
            _iListener = iListener;
        }

        protected override void Ready()
        {
            base.Ready();
            
            _data?.BattleMode?.Begin();
        }
    }
}

