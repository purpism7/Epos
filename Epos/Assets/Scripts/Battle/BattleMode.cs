using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

using VContainer;

using Creature;

namespace Battle
{
    public class BattleMode
    {
        
        public class BaseData
        {
            public List<ICombatant> AllyICombatantList = new();
            public List<ICombatant> EnemyICombatantList = new();
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
        
        public virtual void ChainLateUpdate()
        {
            
        }
        
        public void SetIListener(IListener iListener)
        {
            _iListener = iListener;
        }
    }
    
    public abstract class BattleMode<T> : BattleMode where T : BattleMode.BaseData
    {
        [Inject] protected IObjectResolver _iResolver = null;

        protected T _data = null;

        public virtual BattleMode<T> Initialize(T data)
        {
            _data = data;

            return this;
        }

        public abstract override void Begin();
        public abstract override void ChainUpdate();
        public abstract override void ChainLateUpdate();

        protected virtual void End()
        {
            _iListener?.End();
        }
    }
}

