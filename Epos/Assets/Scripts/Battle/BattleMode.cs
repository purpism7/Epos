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
            public List<ICombatant> AllyICombatantList = null;
            public List<ICombatant> EnemyICombatantList = null;
        }

        public interface IListener
        {
            void End();
        }

        protected IListener _iListener = null;
        
        public virtual void Begin()
        {
            // _iListener = iListener;
            
            Debug.Log("BattleMode Begin");
        }

        public virtual void ChainUpdate()
        {
            
        }
        
        public void SetIListener(IListener iListener)
        {
            _iListener = iListener;
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
        public abstract override void ChainUpdate();

        protected virtual void End()
        {
            _iListener?.End();
        }
    }
}

