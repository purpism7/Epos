using UnityEngine;

using Cysharp.Threading.Tasks;
using System.Collections.Generic;

using VContainer;

using Common;
using Creator;
using Creature;
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
        void OnClickedShout();
        void OnClickAggressive();
        void OnCloseStrategyPanel();
        
        // void CreateEmotion(EmotionType emotionType);
    }

    public class BattleMainPresenter : IBattleMainPresenter, ShoutPanel.IListener
    {
        // [Inject] private UIFactory _uiFactory = null;
        [Inject] private ICameraManager _iCameraManager = null;
        [Inject] private GameSystem.ITimeScaleManager _iTimeScaleManager = null;

        private IBattleMainView _view = null;

        async UniTask IPresenter<BattleMainView>.InitializeAsync(BattleMainView view)
        {
            _view = view;

            await _view.InitializePanelAsync(this);
            await InitializeAllyBattlePortraitList();
        }

        private List<IBattlePortraitSlot> _battlePortraitSlotList = null;

        private async UniTask InitializeAllyBattlePortraitList()
        {
            if (_battlePortraitSlotList == null)
                _battlePortraitSlotList = new();

            _battlePortraitSlotList?.Clear();
            
            for (int i = 0; i < _view?.AllyICombatantList?.Count; ++i)
            {
                var iCombatant = _view?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                var battlePortraitSlotParam = new BattlePortraitSlot.Param(iCombatant);
                var battlePortraitSlot = await _view.CreateBattlePortraitSlotAsync(battlePortraitSlotParam);
                if(battlePortraitSlot != null)
                    _battlePortraitSlotList?.Add(battlePortraitSlot);
            }
        }

        #region IBattleMainPresenter

        public void OnClickedShout()
        {
            for (int i = 0; i < _view?.AllyICombatantList?.Count; ++i)
            {
                var actor = _view?.AllyICombatantList[i]?.IActor;
                if(actor == null)
                    continue;

                if (actor.Id == 10003)
                {
                    _iCameraManager?.SetTargetTm(actor.Transform);
                    break;
                }
            }

            _iCameraManager?.ZoomIn(
                () =>
                {
                    _iTimeScaleManager?.Set(0.2f);
                }, null);

            // _iTimeScaleManager?.Set(0.2f);
        }
        
        void IBattleMainPresenter.OnClickAggressive()
        {
            _iCameraManager.ZoomIn(
                () =>
                {
                    _iTimeScaleManager?.Set(0.2f);
                }, null);
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
        
        #region ShoutPanel.IListener

        void ShoutPanel.IListener.OnSelectShout(EmotionType emotionType)
        {
           // _view?.ActivateEmotionPart(emotionType);

           _iTimeScaleManager?.Set(1f);
           _view?.ActivateBattleMainView();

           for (int i = 0; i < _view?.AllyICombatantList?.Count; ++i)
           {
               var emotionalActor = _view?.AllyICombatantList[i]?.IActor as IEmotionalActor;
               if(emotionalActor == null)
                   continue;

               if (emotionalActor.Id != 10003)
                   continue;
               
               emotionalActor.IEmotionCtr?.UpdateEmotion(emotionType);
           }
           
           for (int i = 0; i < _battlePortraitSlotList?.Count; ++i)
           {
               var slot = _battlePortraitSlotList[i];
               if(slot == null)
                   continue;
               
               slot.UpdateEmotionAsync(emotionType).Forget();
           }

           EventHandler.Notify(new HeroEmotionEventData(10003, emotionType));
        }
        #endregion
    }
}


