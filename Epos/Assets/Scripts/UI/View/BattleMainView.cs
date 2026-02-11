using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;
using VContainer;

using UI.Slot;
using Battle.Step;
using Common;
using Creator;
using Creature;
using UI.Parts;
using UI.Presenter;

namespace UI.View
{
    public interface IBattleMainView : IView
    {
       List<ICombatant> AllyICombatantList { get; }
       RectTransform AllyBattlePortraitRootRectTm { get; }

       UniTask InitializePanelAsync(ShoutPanel.IListener shoutPanelListener);
       
       void ActivateBattleMainView();
       UniTask<IBattlePortraitSlot> CreateBattlePortraitSlotAsync(BattlePortraitSlot.Param param);
    }

    public class BattleMainView : BaseView<BattleMainView.Param>, IBattleMainView
    {
        public class Param : Common.Param
        {
            public List<ICombatant> AllyICombatantList { get; private set; } = null;

            public Param WithAllyICombatantList(List<ICombatant> iCombatantList)
            {
                AllyICombatantList = iCombatantList;
                return this;
            }
        }

        [Inject] private GameSystem.ITimeScaleManager _iTimeScaleManager = null;
        [Inject] private UIFactory _uiFactory = null;
        [Inject] private IObjectResolver _iResolver = null;

        [SerializeField] private RectTransform allyBattlePortraitRootRectTm = null;
        [SerializeField] private Animator animator = null;

        [Header("Panel")]
        [SerializeField] private ShoutPanel shoutPanel = null;
        [SerializeField] private StrategyPanel strategyPanel = null;

        [Header("Button")]
        [SerializeField] private Button shoutBtn = null;
        [SerializeField] private Button aggressiveBtn = null;
        [SerializeField] private Button closeShoutPanelBtn = null;
        [SerializeField] private Button closeStrategyPanelBtn = null;
        
        private IBattleMainPresenter _iPresenter = null;

        public List<ICombatant> AllyICombatantList => _param?.AllyICombatantList;
        public RectTransform AllyBattlePortraitRootRectTm => allyBattlePortraitRootRectTm;

        public override void Configure(IObjectResolver iResolver)
        {
            _iPresenter = RegisterPresenter<BattleMainPresenter>(iResolver);

            iResolver?.Inject(strategyPanel);
        }

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);
            await _iPresenter.InitializeAsync(this);

            InitializeButton();
        }

        private void InitializeButton()
        {
            shoutBtn?.onClick?.AddListener(
                () =>
                {
                    DeactivateAnimBattleMainView();

                    shoutPanel?.Activate();
                    _iTimeScaleManager?.Set(0.2f);
                });

            aggressiveBtn?.onClick?.AddListener(
                () =>
                {
                    DeactivateAnimBattleMainView();

                    strategyPanel?.ActivateAsync(null);
                    _iPresenter.OnClickAggressive();
                });

            closeShoutPanelBtn?.onClick?.AddListener(
                () =>
                {
                    shoutPanel?.Deactivate();
                    _iTimeScaleManager?.Set(1f);

                    ActivateAnimBattleMainView();
                });

            closeStrategyPanelBtn?.onClick?.AddListener(
                () =>
                {
                    _iPresenter?.OnCloseStrategyPanel();

                    strategyPanel?.Deactivate();
                    ActivateAnimBattleMainView();
                });
        }
        
        #region IBattleMainView
        async UniTask IBattleMainView.InitializePanelAsync(ShoutPanel.IListener shoutPanelListener)
        {
            await shoutPanel.InitializeAsync(new ShoutPanel.Param(shoutPanelListener));
            await strategyPanel.InitializeAsync(new StrategyPanel.Param());
        }
        
        void IBattleMainView.ActivateBattleMainView()
        {
            ActivateAnimBattleMainView();
        }

        async UniTask<IBattlePortraitSlot> IBattleMainView.CreateBattlePortraitSlotAsync(BattlePortraitSlot.Param param)
        {
            var uiCreator = _uiFactory?.Create<BattlePortraitSlot, BattlePortraitSlot.Param>(_iResolver);
            if (uiCreator == null)
                return null;
            
            var battlePortraitSlot = await uiCreator
                .SetRoot(allyBattlePortraitRootRectTm)
                .SetParam(param)
                .CreateAsync();
                
            battlePortraitSlot?.ActivateAsync(param);

            return battlePortraitSlot;
        }
        #endregion

        private void ActivateAnimBattleMainView()
        {
            animator?.SetBool("OnOff", false);
        }

        private void DeactivateAnimBattleMainView()
        {
            animator?.SetBool("OnOff", true);
        }

        [Inject]
        private void InjectInitialize()
        {
            Debug.Log("InjectInitialize");
        }
    }
}

