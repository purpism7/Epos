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

    public class HeroEmotionEventData : EventData
    {
        public int CharacterId { get; private set; } = 0;
        public EmotionType EmotionType { get; private set; } = EmotionType.None;

        public HeroEmotionEventData(int characterId, EmotionType emotionType)
        {
            CharacterId = characterId;
            EmotionType = emotionType;
        }
    }
    
    #region Skill
    public class SkillUseEventData : EventData
    {
        public Ability.ISkill ISkill { get; private set; } = null;
        public TeamType TeamType { get; private set; } = TeamType.None;

        public SkillUseEventData WithISkill(Ability.ISkill iSkill)
        {
            ISkill = iSkill;
            return this;
        }

        public SkillUseEventData WithTeamType(TeamType teamType)
        {
            TeamType = teamType;
            return this;
        }
    }
    #endregion 
}

