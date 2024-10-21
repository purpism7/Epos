using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

using Creature;

namespace Battle
{
    public class BattleMode
    {
        public class BaseData
        {
            public List<IActor> AllyIActorList = null;
            public List<IActor> EnemyIActorList = null;
        }

        public virtual void Begin()
        {
            
        }
    }
    
    public abstract class BattleMode<T> : BattleMode where T : BattleMode.BaseData
    {
        protected T _data = null;

        public virtual BattleMode<T> Initialize(T data)
        {
            _data = data;

            return this;
        }

        public abstract override void Begin();
    }
}

