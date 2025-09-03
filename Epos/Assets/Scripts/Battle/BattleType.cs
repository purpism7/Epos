using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle.Step;
using VContainer;

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

        protected IObjectResolver _iResolver = null;


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

        public virtual void ChainLateUpdate()
        {
            
        }

        
        protected virtual void Ready()
        {
            
        }

        protected void BattleEnd()
        {
            _iListener?.End();
        }

        protected void AddStep<V>(BattleStep.BattleStepParam param = null, bool isLast = false) where V : BattleStep, new()
        {
            var step = new V();
            _iResolver?.Inject(step);

            var iBattleStep = step.Initialize(param);

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

        #region BattleMode.IListener
        void BattleMode.IListener.End()
        {
            End();
        }
        #endregion
    }
    
    public abstract class BattleType<T> : BattleType where T : BattleType<T>.BattleTypeParam
    {
        public class BattleTypeParam
        {
            public BattleMode BattleMode { get; private set; } = null;

            public BattleTypeParam(BattleMode battleMode)
            {
                BattleMode = battleMode;
            }
        }
        
        protected T _param = null;

        [Inject]
        private void Initialize(IObjectResolver iResolver)
        {
            Debug.Log("BattleType");
            _iResolver = iResolver;
        }

        public virtual void Initialize(T param)
        {
            _param = param;

            _param?.BattleMode?.SetIListener(this);
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
            
            _param?.BattleMode?.ChainUpdate();
        }

        public override void ChainLateUpdate()
        {
            base.ChainLateUpdate();
            
            _param?.BattleMode?.ChainLateUpdate();
        }
        
        public void SetIListener(IListener iListener)
        {
            _iListener = iListener;
        }

        protected override void Ready()
        {
            base.Ready();
            
            _param?.BattleMode?.Begin();
        }

        
    }
}

