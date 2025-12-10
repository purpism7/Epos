using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

using TMPro;

using UI.Slot;
using UI.Parts;
using GameSystem.Event;
using Common;
using Entities;
using EventHandler = GameSystem.Event.EventHandler;
using Party = Datas.ScriptableObjects.Party;

namespace UI.Parts
{
    public class BattlePartyPart : Part<BattlePartyPart.Param>
    {
        public class Param : Common.Param
        {
            public Datas.ScriptableObjects.Party Party { get; private set; } = null;
            public TeamType TeamType { get; private set; } = TeamType.None;

            public Param WithParty(Party party)
            {
                Party = party;
                return this;
            }
            
            public Param WithTeamType(TeamType teamType)
            {
                TeamType = teamType;
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

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            EventHandler.Add<SkillUseEventData>(OnSkillUse);
            EventHandler.Add<TurnBasedEventData>(OnTurnBased);

            skillNameTMP?.SetText(string.Empty);

            ApplyParty();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            EventHandler.Remove<GameSystem.Event.SkillUseEventData>(OnSkillUse);
            EventHandler.Remove<TurnBasedEventData>(OnTurnBased);
        }

        private void ApplyParty()
        {
            var positionInfos = _param?.Party?.PositionInfos;
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
                    
                    //var battlePortraitSlotParam = new BattlePortraitSlot.Param(positionInfo.CharacterId, eClass);
                    //_battlePortraitSlots[i]?.Activate(battlePortraitSlotParam);
                    
                    break;
                }
            }
        }
        
        private void OnSkillUse(SkillUseEventData eventData)
        {
            //if (eventData?.ISkill == null ||
            //    _param == null)
            //    return;

            //if (eventData.ISkill.ESkillCategory != ESkillCategory.Passive)
            //    return;
            
            //if (eventData.ETeam != _param.ETeam)
            //    return;
            
            //// skillNameTMP?.SetText(string.Empty);
            
            ////Debug.Log(eventData.ISkill.name);
            ////skillNameTMP?.SetText(eventData.ISkill?.name);
        }

        private void OnTurnBased(TurnBasedEventData eventData)
        {
            switch (eventData)
            {
                case StartTurnEventData startTurnEventData:
                {
                    break;
                }
                
                case EndTurnEventData endTurnEventData:
                {
                    skillNameTMP?.SetText(string.Empty);
                    break;
                }
            }
        }
    }
}

