using UnityEngine;

using Creature;
using Datas.ScriptableObjects;

namespace GameSystem.Event
{
    public class StatChangedEventData : EventData
    {
        // public int CharacterId { get; private set; } = 0;
        public IStat IStat { get; private set; } = null; 
        
        public StatChangedEventData(IStat iStat)
        {
            IStat = iStat;
        }
    }

    public class SkillUseEventData : EventData
    {
        public ICaster ICaster { get; private set; } = null;
        public Skill Skill { get; private set; } = null;

        public SkillUseEventData(ICaster iCaster, Skill skill)
        {
            ICaster = iCaster;
            Skill = skill;
        }
    }
}

