using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Creature
{
    public interface ICombatant : ICaster
    {
        void SetETeam(Type.ETeam eTeam);
        public Type.ETeam ETeam { get; }
        
        void SetEFormation(Type.EFormation eFormation);
        public Type.EFormation EFormation { get; }

        int Position { get; }

    }
}
