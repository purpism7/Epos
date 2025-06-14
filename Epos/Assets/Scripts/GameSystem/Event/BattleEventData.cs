using UnityEngine;

using Creature;
using Datas.ScriptableObjects;
using Common;

namespace GameSystem.Event
{
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

    public class SkillUseEventData : EventData
    {
        public Skill Skill { get; private set; } = null;
        public ETeam ETeam { get; private set; } = ETeam.None;

        public SkillUseEventData(Skill skill, ETeam eTeam)
        {
            Skill = skill;
            ETeam = eTeam;
        }
    }
}

