using System.Linq;
using UnityEngine;

using TMPro;

using UI.Slots;
using UI.Parts;

using Datas.ScriptableObjects;
using GameSystem.Event;
using Common;

namespace UI.Parts
{
    public class BattlePartyPart : Part<BattlePartyPart.Data>
    {
        public class Data : Common.ComponentData
        {
            public Datas.ScriptableObjects.Party Party { get; private set; } = null;
            public ETeam ETeam { get; private set; } = ETeam.None;

            public Data WithParty(Party party)
            {
                Party = party;
                return this;
            }
            
            public Data WithETeam(ETeam eTeam)
            {
                ETeam = eTeam;
                return this;
            }
        }
        
        [SerializeField] private TextMeshProUGUI skillNameTMP = null;

        private BattlePortraitSlot[] _battlePortraitSlots = null;
        
        public override void Initialize()
        {
            base.Initialize();
            
            _battlePortraitSlots = rootTm.GetComponentsInChildren<BattlePortraitSlot>();
            
            EventHandler.Add<GameSystem.Event.StatChangedEventData>(OnStatChanged);
            EventHandler.Add<GameSystem.Event.SkillUseEventData>(OnSkillUse);
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            skillNameTMP?.SetText(string.Empty);
            
            ApplyParty();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<GameSystem.Event.StatChangedEventData>(OnStatChanged);
            EventHandler.Remove<GameSystem.Event.SkillUseEventData>(OnSkillUse);
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

        private void OnStatChanged(StatChangedEventData eventData)
        {
            // Debug.Log(eventData.CharaerId);
        }

        private void OnSkillUse(SkillUseEventData eventData)
        {
            if (eventData == null ||
                _data == null)
                return;
            
            if (eventData.ETeam != _data.ETeam)
                return;
            
            Debug.Log(eventData.Skill.name);
            skillNameTMP?.SetText(eventData.Skill?.name);
        }
    }
}

