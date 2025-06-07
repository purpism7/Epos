using UnityEngine;

using TMPro;

using UI.Slots;
using UI.Parts;

using Datas.ScriptableObjects;
using GameSystem.Event;
using EventData = Spine.EventData;

namespace UI.Parts
{
    public class BattlePartyPart : Part<BattlePartyPart.Data>
    {
        public class Data : UI.ComponentData
        {
            public Datas.ScriptableObjects.Party Party { get; private set; } = null;
            
            public Data WithLeftParty(Party party)
            {
                Party = party;
                return this;
            }
        }
        
        [SerializeField] private TextMeshProUGUI skillName = null;

        private BattlePortraitSlot[] _battlePortraitSlots = null;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _battlePortraitSlots = rootTm.GetComponentsInChildren<BattlePortraitSlot>();
            
            EventHandler<GameSystem.Event.BattleCombatantEventData>.Add(OnChanged);
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            ApplyParty();
            
            // EventHandler<>
        }

        private void ApplyParty()
        {
            var positionInfos = _data?.Party?.PositionInfos;
            if (positionInfos.IsNullOrEmpty())
                return;
            
            for (int i = 0; i < _battlePortraitSlots?.Length; ++i)
            {
                _battlePortraitSlots[i]?.Deactivate();
                    
                for (int j = 0; j < positionInfos?.Length; ++j)
                {
                    var positionInfo = positionInfos[j];
                    if(positionInfo == null)
                        continue;
                        
                    if (i != positionInfo.Position - 1)
                        continue;
                        
                    var battlePortraitSlotData = new BattlePortraitSlot.Data()
                        .WithCharacterId(positionInfo.CharacterId);
                            
                    _battlePortraitSlots[i]?.Activate(battlePortraitSlotData);
                    break;
                }
            }
        }

        private void OnChanged(BattleCombatantEventData eventData)
        {
            Debug.Log(eventData.CharacterId);
        }
    }
}

