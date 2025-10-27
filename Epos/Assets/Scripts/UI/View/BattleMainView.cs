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
        
        [SerializeField] private Button shoutBtn = null;
        [SerializeField] private Button aggressiveBtn = null;

        [SerializeField] private StrategyPanel strategyPanel = null;
        [SerializeField] private Button strategyCloseBtn = null;

        private IBattleMainPresenter _iPresenter = null;

        public List<ICombatant> AllyICombatantList => _param?.AllyICombatantList;
        public RectTransform AllyBattlePortraitRootRectTm => allyBattlePortraitRootRectTm;

        public override void CreatePresenter(IObjectResolver iResolver)
        {
            _iPresenter = RegisterPresenter<BattleMainPresenter>(iResolver);
        }

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);
            await _iPresenter.InitializeAsync(this);

            InitializeButton();
            
            strategyPanel?.Deactivate();
        }

        private void InitializeButton()
        {
            shoutBtn?.onClick?.AddListener(() =>
            {
                

                _iPresenter.OnClickShout();
            });

            aggressiveBtn?.onClick?.AddListener(() =>
            {

                strategyPanel?.ActivateAsync(null);
                _iTimeScaleManager?.Set(0.2f);

                //_iPresenter.OnClickAggressive();
            });
            
            strategyCloseBtn?.onClick?.AddListener(() =>
            {
                strategyPanel?.Deactivate();
                _iTimeScaleManager?.Set(1f);
            });
        }

        [Inject]
        private void InjectInitialize()
        {
            Debug.Log("InjectInitialize");
        }
    }
}

