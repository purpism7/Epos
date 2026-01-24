using UnityEngine;

using Cysharp.Threading.Tasks;
using System.Collections.Generic;

using VContainer;

using Common;
using Creator;
using UI.Slot;
using UI.View;
using UI.Popup;
using GameSystem;
using GameSystem.Event;
using UI.Parts;


namespace UI.Presenter
{
    public interface IBattleMainPresenter : IPresenter<BattleMainView>
    {
        void OnClickAggressive();
        void OnCloseStrategyPanel();
        
        void CreateEmotion(EmotionType emotionType);
    }

    public class BattleMainPresenter : IBattleMainPresenter, ShoutPanel.IListener
    {
        [Inject] private UIFactory _uiFactory = null;
        [Inject] private ICameraManager _iCameraManager = null;
        [Inject] private GameSystem.ITimeScaleManager _iTimeScaleManager = null;

        private IBattleMainView _view = null;

        async UniTask IPresenter<BattleMainView>.InitializeAsync(BattleMainView view)
        {
            _view = view;

            await _view.InitializePanelAsync(this);
            await InitializeAllyBattlePortraitList();
        }

        private List<BattlePortraitSlot> _battlePortraitSlotList = null;

        private async UniTask InitializeAllyBattlePortraitList()
        {
            if (_battlePortraitSlotList == null)
                _battlePortraitSlotList = new();

            _battlePortraitSlotList?.Clear();

            var uiCreator = _uiFactory?.Create<BattlePortraitSlot, BattlePortraitSlot.Param>();

            for (int i = 0; i < _view?.AllyICombatantList?.Count; ++i)
            {
                var iCombatant = _view?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                var battlePortraitSlotParam = new BattlePortraitSlot.Param(iCombatant);

                var battlePortraitSlot = await uiCreator
                    .SetRoot(_view?.AllyBattlePortraitRootRectTm)
                    .SetParam(battlePortraitSlotParam)
                    .CreateAsync();
                
                battlePortraitSlot?.ActivateAsync(battlePortraitSlotParam);

                _battlePortraitSlotList?.Add(battlePortraitSlot);
            }
        }

        #region IBattleMainPresenter
        void IBattleMainPresenter.OnClickAggressive()
        {
            _iCameraManager.ZoomIn(
                () =>
                {
                    _iTimeScaleManager?.Set(0.2f);
                },
                null);
        }

        void IBattleMainPresenter.OnCloseStrategyPanel()
        {
            _iTimeScaleManager?.Set(1f);

            _iCameraManager.ZoomOut(
              () =>
              {
                 
              });
        }

        void IBattleMainPresenter.CreateEmotion(EmotionType emotionType)
        {
            
        }
        #endregion
        
        #region ShoutPanel.IListener

        void ShoutPanel.IListener.OnSelectShout(EmotionType emotionType)
        {
           // _view?.ActivateEmotionPart(emotionType);

           _iTimeScaleManager?.Set(1f);
           _view?.ActivateBattleMainView();
            
           EventHandler.Notify(new HeroEmotionEventData(10003, emotionType));
        }
        #endregion
    }
}


