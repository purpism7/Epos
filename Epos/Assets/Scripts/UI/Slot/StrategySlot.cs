using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine;
using TMPro;

using Battle.Strategy;
using Common;

namespace UI.Slot
{
    public interface IStrategySlot
    {
        IStrategy IStrategy { get; }
        string StrategyPhase { get; }

        void Select();
        void Deselect();
    }

    public class StrategySlot : BaseSlot<StrategySlot.Param>, IStrategySlot
    {
        public class Param : Common.Param
        {
            public IListener IListener { get; private set; } = null;
            public IStrategy IStrategy { get; private set; } = null;

            public Datas.ScriptableObjects.Strategy StrategyData { get; private set; } = null;

            public Param(IStrategy iStrategy, IListener iListener)
            {
                IStrategy = iStrategy;
                IListener = iListener;
            }

            public Param WithStrategyData(Datas.ScriptableObjects.Strategy strategyData)
            {
                StrategyData = strategyData;
                return this;
            }
        }

        public interface IListener
        {
            void OnSelectStrategy(IStrategySlot iStrategySlot);
        }

        [SerializeField] private Animator animator = null;
        //[SerializeField] private RectTransform selectRootRectTm = null;
        [SerializeField] private TextMeshProUGUI strategyTypeNameTMP = null;
        [SerializeField] private TextMeshProUGUI strategyEffectDescriptionTMP = null;
        [SerializeField] private TextMeshProUGUI strategyDesctionTMP = null;
        [SerializeField] private UnityEngine.UI.Button selectBtn = null;

        private bool _isSelected = false;

        public override async UniTask InitializeAsync(Param param = null)
        {
            await base.InitializeAsync(param);

            strategyTypeNameTMP?.SetText(_param?.StrategyData?.StrategyTypeName);
            strategyEffectDescriptionTMP?.SetText(_param?.StrategyData?.EffectDescription);
            strategyDesctionTMP?.SetText(_param?.StrategyData?.Description);

            selectBtn?.onClick?.RemoveAllListeners();
            selectBtn?.onClick?.AddListener(OnClickSelect);
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);
            
            return UniTask.CompletedTask;
        }

        private void Select()
        {
            animator?.SetBool("Select", true);

            _isSelected = true;
        }

        private void OnClickSelect()
        {
            if (_isSelected)
                return;

            Select();
    
            _param?.IListener?.OnSelectStrategy(this);
        }

        #region IStrategySlot
        IStrategy IStrategySlot.IStrategy
        {
            get { return _param?.IStrategy; }
        }

        string IStrategySlot.StrategyPhase
        {
            get { return _param?.StrategyData?.Phases; }
        }

        void IStrategySlot.Select()
        {
            Select();
        }

        void IStrategySlot.Deselect()
        {
            animator?.SetBool("Select", false);

            _isSelected = false;
        }
        #endregion
    }
}