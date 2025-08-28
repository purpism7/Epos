using System.Collections.Generic;
using UnityEngine;


using Cysharp.Threading.Tasks;
using VContainer;

using UI.Slots;
using UnityEngine.UIElements.Experimental;
using Battle.Step;

namespace UI.View
{
    public class BattleMainView : Common.Component<BattleMainView.Param>
    {
        public class Param : Common.Param
        {
            public Datas.ScriptableObjects.Party AllyParty { get; private set; } = null;

            public Param WithAllyParty(Datas.ScriptableObjects.Party allyParty)
            {
                AllyParty = allyParty;
                return this;
            }
        }

        private List<BattlePortraitSlot> _battlePortraitSlotList = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        [Inject]
        private void InjectInitialize()
        {
            Debug.Log("InjectInitialize");
        }
    }
}

