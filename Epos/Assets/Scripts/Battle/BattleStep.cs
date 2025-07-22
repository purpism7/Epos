using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace Battle
{
    public interface IBattleStep
    {
        void Begin();
        void SetChainStep(BattleStep step);
        void SetLastStepEndAction(System.Action endAction);
    }
    
    public class BattleStep : IBattleStep
    {
        public class BattleStepParam
        {
            
        }
        
        private BattleStep _chainStep = null;
        private System.Action _lastStepEndAction = null;

        public virtual IBattleStep Initialize(BattleStepParam param)
        {
            return this;
        }
        
        public virtual void Begin()
        {
            
        }

        protected virtual void End()
        {
            BeginChainStepAsync().Forget();
        }

        private async UniTask BeginChainStepAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            Debug.Log(GetType());
            
            _chainStep?.Begin();
            _lastStepEndAction?.Invoke();
        }
        
        void IBattleStep.SetChainStep(BattleStep step)
        {
            _chainStep = step;
        }
        
        void IBattleStep.SetLastStepEndAction(System.Action endAction)
        {
            _lastStepEndAction = endAction;
        }
    }
    
    public abstract class BattleStep<T> : BattleStep where T : BattleStep.BattleStepParam
    {
        protected T _param = null;

        public override IBattleStep Initialize(BattleStepParam param)
        {
            base.Initialize(param);

            _param = param as T;

            return this;
        }

        public abstract override void Begin();
    }
}

