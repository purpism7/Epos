using System;
using System.Linq;
using UnityEngine;

using TMPro;

using UI.Slots;
using UI.Parts;
using GameSystem.Event;
using Common;
using Entities;
using EventHandler = GameSystem.Event.EventHandler;
using Party = Datas.ScriptableObjects.Party;

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
        }

        public override void Activate(Data data)
        {
            base.Activate(data);

            EventHandler.Add<GameSystem.Event.SkillUseEventData>(OnSkillUse);
            
            skillNameTMP?.SetText(string.Empty);
            
            ApplyParty();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
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
                    
                    // 데이터화
                    EClass eClass = EClass.None;
                    switch (positionInfo.CharacterId)
                    {
                        case 10001:
                        case 10002:
                            eClass = EClass.Knight;
                            break;
                        
                        case 10003:
                            eClass = EClass.Wizard;
                            break;
                        
                        case 10004:
                            eClass = EClass.Assassin;
                            break;
                    }
                    
                    var battlePortraitSlotData = new BattlePortraitSlot.Data(positionInfo.CharacterId, eClass);
                    _battlePortraitSlots[i]?.Activate(battlePortraitSlotData);
                    
                    break;
                }
            }
        }
        
        private void OnSkillUse(SkillUseEventData eventData)
        {
            skillNameTMP?.SetText(string.Empty);
            
            if (eventData?.Skill == null ||
                _data == null)
                return;

            if (eventData.Skill.ESkillCategory != ESkillCategory.Passive)
                return;
            
            if (eventData.ETeam != _data.ETeam)
                return;
            
            Debug.Log(eventData.Skill.name);
            skillNameTMP?.SetText(eventData.Skill?.name);
        }
    }
}

