using UnityEngine;

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Battle.RealTime;
using Battle.Strategy;
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
    }

    public class BattleMainPresenter : IBattleMainPresenter, 
        ShoutPanel.IListener
    {
        [Inject] private ICameraManager _cameraManager = null;
        [Inject] private GameSystem.ITimeScaleManager _timeScaleManager = null;
        [Inject] private IStrategyController _strategyController = null; 

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
            if (_cameraManager == null)
                return;

            var allyCombatants = _view?.AllyICombatantList;
            if (allyCombatants == null)
                return;

            for (int i = 0; i < allyCombatants.Count; ++i)
            {
                var actor = allyCombatants[i]?.Actor;
                if(actor == null)
                    continue;

                if (actor.Id == 10003)
                {
                    _cameraManager.SetTargetTr(actor.Transform, new Vector3(-5f, 0, 0));
                    break;
                }
            }

            _cameraManager.FocusOnTarget(
                () =>
                {
                    _timeScaleManager?.Set(0.2f);
                }, 15f);
        }
        
        void IBattleMainPresenter.OnClickAggressive()
        {
            _cameraManager.FocusOnTarget(
                () =>
                {
                    //_timeScaleManager?.Set(0.2f);
                }, 25f);
        }

        void IBattleMainPresenter.OnCloseStrategyPanel()
        {
            _cameraManager?.ClearFocus(
                () =>
                {
                    _timeScaleManager?.Set(1f);         
                });
        }
        #endregion
        
        #region ShoutPanel.IListener

        void ShoutPanel.IListener.OnSelectShout(EmotionType emotionType)
        {
            _cameraManager?.SetTargetTr(_strategyController?.LeaderCombatant?.Transform, Vector3.zero);
            _cameraManager?.ClearFocus(
               () =>
               {
                   _timeScaleManager?.Set(1f);
                   _view?.ActivateBattleMainView();

                   var allyCombatants = _view?.AllyICombatantList;
                   if (allyCombatants == null)
                       return;

                   for (int i = 0; i < allyCombatants.Count; ++i)
                   {
                       var emotionalActor = _view?.AllyICombatantList[i]?.Actor as IEmotionalActor;
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
               });
        }
        #endregion
    }
}

