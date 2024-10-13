using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class BattleMode
    {
        
    }
    
    public abstract class BattleMode<T> : BattleMode where T : BattleMode<T>.BaseData
    {
        public class BaseData
        {
            
        }

        protected T _data = null;
        
        public abstract BattleMode<T> Initialize(T data);
    }
}

