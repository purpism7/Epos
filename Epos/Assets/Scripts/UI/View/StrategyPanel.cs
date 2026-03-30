using UnityEngine;
using System;

using VContainer;
using Cysharp.Threading.Tasks;
using TMPro;

using UI.Slot;
using Battle;
using Battle.Strategy;

namespace UI.View
{
    public class StrategyPanel : Common.Component<StrategyPanel.Param>, StrategySlot.IListener
    {
        public class Param : Common.Param
        {
            public IListener Listener { get; private set; } = null;

            public Param(IListener listener)
            {
                Listener = listener;
            }
        }

        public interface IListener
        {
            UniTask OnConfirmStrategyAsync();
        }
        
        [SerializeField] private Animator animator = null;

        // TODO: Data
        [SerializeField] private Datas.ScriptableObjects.Strategy[] strategyDatas = null;
        [SerializeField] private TextMeshProUGUI strategyPhaseTMP = null;

        [Inject] IStrategyController _strategyController = null;

        private StrategySlot[] _strategySlots = null;
        private IStrategySlot _currentIStrategySlot = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);

            _strategySlots = GetComponentsInChildren<StrategySlot>(true);

            await _strategySlots[0].InitializeAsync(new StrategySlot.Param(new Adaptive(), 1, this).WithStrategyData(strategyDatas[0]));
            await _strategySlots[1].InitializeAsync(new StrategySlot.Param(new Offensive(), 2, this).WithStrategyData(strategyDatas[1]));
            await _strategySlots[2].InitializeAsync(new StrategySlot.Param(new Defensive(), 3, this).WithStrategyData(strategyDatas[2]));

            _currentIStrategySlot = _strategySlots[0];
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

            int index = 0;
            switch (_strategyController.CurrentIStrategy)
            {
                case Adaptive:
                    index = 1;
                    break;
                
                case Offensive:
                    index = 2;
                    break;
                
                case Defensive:
                    index = 3;
                    break;
            }
            
            animator?.SetBool("Out", false);
            animator?.SetInteger("Select", index);

            await UniTask.Yield();
            // _currentIStrategySlot?.Select();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            // strategyPhaseTMP?.SetText(_currentIStrategySlot?.StrategyPhase);
        
            // animator?.SetBool("OnOff", true);
        }

        #region StrategySlot.IListener
        void StrategySlot.IListener.OnSelectStrategy(IStrategySlot strategySlot)
        {
            if (strategySlot == null)
                return;

            if (_strategyController == null)
                return;

            if (_strategyController.CurrentIStrategy == strategySlot.IStrategy)
                return;
            
            animator?.SetInteger("Select", strategySlot.Index);
            
            _strategyController.ApplyStrategy(strategySlot.IStrategy);

            _currentIStrategySlot?.Deselect();
            _currentIStrategySlot = strategySlot;
        }

        void StrategySlot.IListener.OnConfirmStrategy()
        {
            // animator?.SetInteger("Select", strategySlot.Index);
            animator?.SetBool("Out", true);
            //
            // Deactivate();
            _param?.Listener?.OnConfirmStrategyAsync();
        }
        #endregion
    }
}

