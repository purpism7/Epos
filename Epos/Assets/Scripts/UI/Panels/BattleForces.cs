using System.Collections;
using System.Collections.Generic;
using Creator;
using Datas.ScriptableObjects;
using UnityEngine;

using GameSystem;
using Parts;
using UI.Parts;
using UI.Slots;

namespace UI.Panels
{
    public class BattleForces : UI.Panel<BattleForces.Data>
    {
        public class Data : UI.ComponentData
        {
            public PartyLocation LeftPartyLocation { get; private set; } = null;
            public PartyLocation RightPartyLocation = null;

            public Party LeftParty { get; private set; } = null;

            public Data WithLeftPartyLocation(PartyLocation partyLocation)
            {
                LeftPartyLocation = partyLocation;
                return this;
            }
            
            public Data WithRightPartyLocation(PartyLocation partyLocation)
            {
                RightPartyLocation = partyLocation;
                return this;
            }

            public Data WithLeftParty(Party party)
            {
                LeftParty = party;
                return this;
            }
        }
        
        [SerializeField] private RectTransform leftRootRectTm = null;
        [SerializeField] private RectTransform rightRootRectTm = null;
        
        private BattlePortraitSlot[] _leftBattlePortraitSlots = null;
        private BattlePortraitSlot[] _rightBattlePortraitSlots = null;
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);

            _leftBattlePortraitSlots = leftRootRectTm.GetComponentsInChildren<BattlePortraitSlot>();
            _rightBattlePortraitSlots = rightRootRectTm.GetComponentsInChildren<BattlePortraitSlot>();
        }

        public override void Activate(Data data)
        {
            base.Activate(data);
            
            var leftPositionInfos = data?.LeftParty?.PositionInfos;
            if (!leftPositionInfos.IsNullOrEmpty())
            {
                for (int i = 0; i < _leftBattlePortraitSlots?.Length; ++i)
                {
                    _leftBattlePortraitSlots[i]?.Deactivate();
                    
                    for (int j = 0; j < leftPositionInfos?.Length; ++j)
                    {
                        var position = leftPositionInfos[j].Position;
                        if (i == position - 1)
                        {
                            _leftBattlePortraitSlots[i]?.Activate();
                            break;
                        }
                    }
                }
            }
        }
        
        

        public override void Deactivate()
        {
            base.Deactivate();

            // rightFrontRootRectTm.RemoveAllChild();
            // rightRearRootRectTm.RemoveAllChild();
            // leftFrontRootRectTm.RemoveAllChild();
            // leftRearRootRectTm.RemoveAllChild();
        }
    }
}

