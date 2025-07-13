using UnityEngine;

using Creature;
using Datas.ScriptableObjects;
using Common;

namespace GameSystem.Event
{
    #region
    public class TurnBasedEventData : EventData
    {
        public int Turn { get; private set; } = 0;

        protected TurnBasedEventData(int turn)
        {
            Turn = turn;
        }
    }

    public class StartTurnEventData : TurnBasedEventData
    {
        public StartTurnEventData(int turn) : base(turn)
        {
            
        }
    }
    
    public class EndTurnEventData : TurnBasedEventData
    {
        public EndTurnEventData(int turn) : base(turn)
        {
            
        }
    }
    #endregion
    
    public class StatChangedEventData : EventData
    {
        public int CharacterId { get; private set; } = 0;
        public IStat IStat { get; private set; } = null; 
        
        public StatChangedEventData(int characterId, IStat iStat)
        {
            CharacterId = characterId;
            IStat = iStat;
        }
    }
    
    #region Skill
    public class SkillUseEventData : EventData
    {
        public Skill Skill { get; private set; } = null;
        public ETeam ETeam { get; private set; } = ETeam.None;

        public SkillUseEventData WithSkill(Skill skill)
        {
            Skill = skill;
            return this;
        }

        public SkillUseEventData WithETeam(ETeam eTeam)
        {
            ETeam = eTeam;
            return this;
        }
    }
    #endregion 
}

