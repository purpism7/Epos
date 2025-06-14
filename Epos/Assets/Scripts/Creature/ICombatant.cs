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
        
        void SetEFormation(EFormation eFormation);
        public EFormation EFormation { get; }
        
        int PartyPosition { get; }
        void SetPosition(Vector3 position);
    }
}
