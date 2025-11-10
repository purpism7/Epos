using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;
using VContainer;

using UI.Slot;
using Battle.Step;
using Creator;
using Creature;
using UI.Presenter;

namespace UI.View
{
    public interface IBattleMainView : IView
    {
       List<ICombatant> AllyICombatantList { get; }
       RectTransform AllyBattlePortraitRootRectTm { get; }
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

            await shoutPanel.InitializeAsync(null);
            await strategyPanel.InitializeAsync(new StrategyPanel.Param());
            
            InitializeButton();
            
            //strategyPanel?.Deactivate();
            //shoutPanel?.Deactivate();
        }

        private void InitializeButton()
        {
            shoutBtn?.onClick?.AddListener(
                () =>
                {
                    DeactivateAnimBattleMainView();

                    shoutPanel?.ActivateAsync(null);
                    _iTimeScaleManager?.Set(0.2f);
                    //_iPresenter.OnClickShout();
                });

            aggressiveBtn?.onClick?.AddListener(
                () =>
                {
                    DeactivateAnimBattleMainView();

                    strategyPanel?.ActivateAsync(null);
                    _iTimeScaleManager?.Set(0.2f);
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
                    strategyPanel?.Deactivate();
                    _iTimeScaleManager?.Set(1f);

                    ActivateAnimBattleMainView();
                });
        }

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

