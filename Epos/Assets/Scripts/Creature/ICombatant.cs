using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Creature
{
    public interface ICombatant : ICaster
    {
        public class Data
        {
            public EType.EFormation EFormation = EType.EFormation.None;
        }
        
        void Initialize(Data data);
        public EType.EFormation EFormation { get; }
    }
}
