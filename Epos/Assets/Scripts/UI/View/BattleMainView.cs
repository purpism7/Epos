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

        [SerializeField] private RectTransform allyBattlePortraitRootRectTm = null;
        [SerializeField] private Button shoutBtn = null;

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
        }

        private void InitializeButton()
        {
            shoutBtn?.onClick?.AddListener(() =>
            {
                _iPresenter.OnClickShout();
            });
        }

        [Inject]
        private void InjectInitialize()
        {
            Debug.Log("InjectInitialize");
        }
    }
}

