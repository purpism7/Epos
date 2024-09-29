using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        public void Begin<T>() where T : Battle.BattleType, new();
    }
    
    public class BattleManager : Manager, IBattleManager, BattleType.IListener
    {
        private Dictionary<System.Type, Battle.BattleType> _battleTypeDic = null;
        private Battle.BattleType _currBattle = null;
        
        public override IManagerGeneric Initialize()
        {
            return this;
        }

        public void Begin<T>() where T : Battle.BattleType, new()
        {
            if (_currBattle != null)
                return;
            
            if (_battleTypeDic == null)
            {
                _battleTypeDic = new();
                _battleTypeDic.Clear();
            }

            Battle.BattleType battleType = null;
            if (!_battleTypeDic.TryGetValue(typeof(T), out battleType))
            {
                battleType = new T();
                battleType.Initialize(this);
       
                _battleTypeDic?.TryAdd(typeof(T), battleType);
            }

            if (battleType == null)
            {
                Debug.Log("No Create = " + typeof(T));
                
                return;
            }
            
            battleType.Begin();

            _currBattle = battleType;
        }
        
        #region BattleType.IListener

        void BattleType.IListener.End()
        {
            
        }
        #endregion
    }
}

