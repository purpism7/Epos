using UnityEngine;

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using VContainer;

using Creator;
using UI.Slot;
using UI.View;
using UI.Popup;
using GameSystem;


namespace UI.Presenter
{
    public interface IBattleMainPresenter : IPresenter<BattleMainView>
    {
        void OnClickShout();
        void OnClickAggressive();
        void OnCloseStrategyPanel();
    }

    public class BattleMainPresenter : IBattleMainPresenter, StrategyPanel.IListener
    {
        [Inject] private UIFactory _uiFactory = null;
        [Inject] private ICameraManager _iCameraManager = null;
        [Inject] private GameSystem.ITimeScaleManager _iTimeScaleManager = null;

        private IBattleMainView _iBattleMainView = null;

        async UniTask IPresenter<BattleMainView>.InitializeAsync(BattleMainView battleMainView)
        {
            _iBattleMainView = battleMainView;

            await InitializeAllyBattlePortraitList();
        }

        private List<BattlePortraitSlot> _battlePortraitSlotList = null;

        private async UniTask InitializeAllyBattlePortraitList()
        {
            if (_battlePortraitSlotList == null)
                _battlePortraitSlotList = new();

            _battlePortraitSlotList?.Clear();

            var uiCreator = _uiFactory?.Create<BattlePortraitSlot, BattlePortraitSlot.Param>();

            for (int i = 0; i < _iBattleMainView?.AllyICombatantList?.Count; ++i)
            {
                var iCombatant = _iBattleMainView?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                var battlePortraitSlotParam = new BattlePortraitSlot.Param(iCombatant);

                var battlePortraitSlot = await uiCreator
                    .SetRoot(_iBattleMainView?.AllyBattlePortraitRootRectTm)
                    .SetParam(battlePortraitSlotParam)
                    .CreateAsync();

                battlePortraitSlot?.ActivateAsync(battlePortraitSlotParam);

                _battlePortraitSlotList?.Add(battlePortraitSlot);
            }
        }

        #region IBattleMainPresenter
        void IBattleMainPresenter.OnClickShout()
        {
            var uiCreator = _uiFactory?.Create<ShoutPopup, ShoutPopup.Param>();
            var popup = uiCreator
                .SetParam(new ShoutPopup.Param())?
                .Create();
            popup?.Activate();
        }

        void IBattleMainPresenter.OnClickAggressive()
        {
            _iCameraManager.ZoomIn(
                () =>
                {
                    _iTimeScaleManager?.Set(0.2f);
                },
                null);

            //var uiCreator = _uiFactory?.Create<TacticalStancePopup, TacticalStancePopup.Param>();
            //var popup = uiCreator
            //    .SetParam(new TacticalStancePopup.Param())?
            //    .Create();
            //popup?.Activate();
        }

        void IBattleMainPresenter.OnCloseStrategyPanel()
        {
            _iTimeScaleManager?.Set(1f);
            _iCameraManager.ZoomOut(
              () =>
              {
                 
              });
        }
        #endregion
    }
}


