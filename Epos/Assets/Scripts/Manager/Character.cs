using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

namespace Manager
{
    public interface ICharacterManager : IManager
    {
   
    }

    public class Character : ICharacterManager
    {
        public IGeneric Initialize()
        {
            return this;
        }

        void IGeneric.ChainUpdate()
        {
            
        }
        
        void IGeneric.ChainLateUpdate()
        {
            
        }
    }
}

