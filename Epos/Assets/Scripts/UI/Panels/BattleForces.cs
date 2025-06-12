using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Datas.ScriptableObjects;
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
        
        [Header("Left")]
        [SerializeField] private BattlePartyPart leftBattlePartyPart = null;
        
        [Header("Right")]
        [SerializeField] private BattlePartyPart rightBattlePartyPart = null;
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);

            leftBattlePartyPart?.Initialize();
            rightBattlePartyPart?.Initialize();
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            var leftBattlePartyPartData = new BattlePartyPart.Data();
            leftBattlePartyPartData.WithParty(data?.LeftParty);
            leftBattlePartyPart?.Activate(leftBattlePartyPartData);
            
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

