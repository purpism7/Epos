using UnityEngine;

using Cysharp.Threading.Tasks;
using System.Collections.Generic;

using Creator;
using UI.Slot;
using UI.View;
using UI.Popup;


namespace UI
{
    public interface IBattleMainPresenter : IPresenter<BattleMainView>
    {
        //UniTask InitializeAsync(BattleMainView battleMainView);
        void OnClickShout();
    }

    public class BattleMainPresenter : IBattleMainPresenter
    {

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

            for (int i = 0; i < _iBattleMainView?.AllyICombatantList?.Count; ++i)
            {
                var iCombatant = _iBattleMainView?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                var battlePortraitSlotParam = new BattlePortraitSlot.Param(iCombatant);

                var battlePortraitSlot = await UICreator<BattlePortraitSlot, BattlePortraitSlot.Param>.Get
                    .SetRoot(_iBattleMainView?.AllyBattlePortraitRootRectTm)
                    .SetParam(battlePortraitSlotParam)
                    .CreateAsync();

                battlePortraitSlot?.Activate(battlePortraitSlotParam);

                _battlePortraitSlotList?.Add(battlePortraitSlot);
            }
        }

        void IBattleMainPresenter.OnClickShout()
        {
            var shoutPopup = UICreator<ShoutPopup, ShoutPopup.Param>.Get?
                .SetParam(new ShoutPopup.Param())?
                .Create();
        }
    }
}

