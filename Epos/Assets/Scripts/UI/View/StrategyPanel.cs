using UnityEngine;
using System;

using VContainer;
using Cysharp.Threading.Tasks;

using UI.Slot;
using Battle;
using Battle.Strategy;
using TMPro;

namespace UI.View
{
    public class StrategyPanel : Common.Component<StrategyPanel.Param>, StrategySlot.IListener
    {
        public class Param : Common.Param
        {

        }

        [SerializeField] private Animator animator = null;

        // TODO: Data
        [SerializeField] private Datas.ScriptableObjects.Strategy[] strategyDatas = null;
        [SerializeField] private TextMeshProUGUI strategyPhaseTMP = null;

        [Inject] IStrategyController _iStrategyController = null;

        private StrategySlot[] _strategySlots = null;
        private IStrategySlot _currentIStrategySlot = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);

            _strategySlots = GetComponentsInChildren<StrategySlot>(true);

            await _strategySlots[0].InitializeAsync(new StrategySlot.Param(new Adaptive(), this).WithStrategyData(strategyDatas[0]));
            await _strategySlots[1].InitializeAsync(new StrategySlot.Param(new Offensive(), this).WithStrategyData(strategyDatas[1]));
            await _strategySlots[2].InitializeAsync(new StrategySlot.Param(new Defensive(), this).WithStrategyData(strategyDatas[2]));

            _currentIStrategySlot = _strategySlots[0];
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

            animator?.SetBool("OnOff", false);

            await UniTask.Yield();
            _currentIStrategySlot?.Select();
        }

        public override void Deactivate()
        {
            //base.Deactivate();
            strategyPhaseTMP?.SetText(_currentIStrategySlot?.StrategyPhase);

            animator?.SetBool("OnOff", true);
        }

        #region StrategySlot.IListener
        void StrategySlot.IListener.OnSelectStrategy(IStrategySlot iStrategySlot)
        {
            if (iStrategySlot == null)
                return;

            if (_iStrategyController == null)
                return;

            if (_iStrategyController.CurrentIStrategy == iStrategySlot.IStrategy)
                return;

            _iStrategyController.ApplyStrategy(iStrategySlot.IStrategy);

            _currentIStrategySlot?.Deselect();
            _currentIStrategySlot = iStrategySlot;
        }
        #endregion
    }
}

