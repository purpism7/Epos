using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;
using Creature.Action;


namespace Creature
{
    public interface ICombatant : ICaster
    {
        IActor IActor { get; }
        
        ETeam ETeam { get; }
        //int PartyPosition { get; }

        void SetETeam(ETeam eTeam);
        // void SetPartyPosition(int partPosition);
        void SetPosition(Vector3 position);
    }
}
