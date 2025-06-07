using UnityEngine;

namespace GameSystem.Event
{
    public class BattleCombatantEventData : EventData
    {
        public int CharacterId { get; private set; } = 0;

        public BattleCombatantEventData WithCharacterId(int characterId)
        {
            CharacterId = characterId;
            return this;
        }
    }
}

