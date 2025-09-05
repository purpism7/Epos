using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;


namespace Creature
{
    public interface ICombatant : ICaster
    {
        void SetETeam(ETeam eTeam);
        public ETeam ETeam { get; }
        
        int PartyPosition { get; }
        void SetPartyPosition(int partPosition);
        void SetPosition(Vector3 position);
    }
}
