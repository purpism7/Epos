using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Creature
{
    public interface ICombatant : ICaster
    {
        void SetEFormation(EType.EFormation eFormation);
        public EType.EFormation EFormation { get; }
    }
}
