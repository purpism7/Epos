using System.Collections.Generic;
using UnityEngine;


using Cysharp.Threading.Tasks;
using VContainer;

using UI.Slots;
using UnityEngine.UIElements.Experimental;
using Battle.Step;
using Creator;
using System.Threading.Tasks;
using Creature;

namespace UI.View
{
    public class BattleMainView : Common.Component<BattleMainView.Param>
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

        private List<BattlePortraitSlot> _battlePortraitSlotList = null;

        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);
            await InitializeAllyBattlePortraitList();
        }

        [Inject]
        private void InjectInitialize()
        {
            Debug.Log("InjectInitialize");
        }

        private async UniTask InitializeAllyBattlePortraitList()
        {
            if (_battlePortraitSlotList == null)
                _battlePortraitSlotList = new();

            _battlePortraitSlotList?.Clear();

            for (int i = 0; i < _param?.AllyICombatantList?.Count; ++i)
            {
                var iCombatant = _param?.AllyICombatantList[i];
                if (iCombatant == null)
                    continue;

                var battlePortraitSlotParam = new BattlePortraitSlot.Param(iCombatant);

                var battlePortraitSlot = await UICreator<BattlePortraitSlot, BattlePortraitSlot.Param>.Get
                    .SetRoot(allyBattlePortraitRootRectTm)
                    .SetParam(battlePortraitSlotParam)
                    .CreateAsync();

                battlePortraitSlot?.Activate(battlePortraitSlotParam);

                _battlePortraitSlotList?.Add(battlePortraitSlot);
            }
        }
    }
}

