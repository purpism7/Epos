using Battle.Strategy;
using Common;
using Cysharp.Threading.Tasks;
using Spine;
using TMPro;
using TMPro.Examples;
using UnityEngine;

namespace UI.Slot
{
    public interface IStrategySlot
    {
        IStrategy IStrategy { get; }

        void Deselect();
    }

    public class StrategySlot : BaseSlot<StrategySlot.Param>, IStrategySlot
    {
        public class Param : Common.Param
        {
            public IListener IListener { get; private set; } = null;
            public IStrategy IStrategy { get; private set; } = null;

            public Param(IStrategy iStrategy, IListener iListener)
            {
                IStrategy = iStrategy;
                IListener = iListener;
                //StrategyType = strategyType;
            }
        }

        public interface IListener
        {
            void OnSelectStrategy(IStrategySlot iStrategySlot);
        }

        [SerializeField] private Animator animator = null;
        //[SerializeField] private RectTransform selectRootRectTm = null;
        [SerializeField] private TextMeshProUGUI strategyTypeNameTMP = null;
        [SerializeField] private UnityEngine.UI.Button selectBtn = null;

        public override async UniTask InitializeAsync(Param param = null)
        {
            await base.InitializeAsync(param);

            strategyTypeNameTMP?.SetText(_param?.IStrategy.GetType().Name);

            selectBtn?.onClick?.RemoveAllListeners();
            selectBtn?.onClick?.AddListener(OnClickSelect);
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);
            
            return UniTask.CompletedTask;
        }

        private void OnClickSelect()
        {

            animator?.SetBool("Select", true);
            //Extensions.SetActive(selectRootRectTm, true);

            _param?.IListener?.OnSelectStrategy(this);
        }

        #region IStrategySlot
        IStrategy IStrategySlot.IStrategy
        {
            get { return _param?.IStrategy; }
        }

        void IStrategySlot.Deselect()
        {
            animator?.SetBool("Select", false);
            //Extensions.SetActive(selectRootRectTm, false);
        }
        #endregion
    }
}