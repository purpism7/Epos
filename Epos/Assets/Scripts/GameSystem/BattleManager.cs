using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle;
using Manager;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        public void Begin<T, V>(V data = null) where T : Battle.BattleType, new() where V : BattleType<V>.BaseData;
    }
    
    public class BattleManager : IBattleManager, BattleType.IListener
    {
        private Dictionary<System.Type, BattleType> _battleTypeDic = null;
        private Battle.BattleType _currBattleType = null;

        // private Dictionary<System.Type, BattleMode> _battleModeDic = null;
        
        public IGeneric Initialize()
        {
            return this;
        }

        public void Begin<T, V>(V data = null) where T : Battle.BattleType, new() where V : BattleType<V>.BaseData
        {
            if (_currBattleType != null)
                return;
            
            if (_battleTypeDic == null)
            {
                _battleTypeDic = new();
                _battleTypeDic.Clear();
            }

            Battle.BattleType battleType = null;
            if (!_battleTypeDic.TryGetValue(typeof(T), out battleType))
            {
                var aBattleType = new T() as BattleType<V>;
                aBattleType?.Initialize(data);
                aBattleType?.SetIListener(this);
       
                _battleTypeDic?.TryAdd(typeof(T), aBattleType);

                battleType = aBattleType;
            }

            if (battleType == null)
            {
                Debug.Log("No Create = " + typeof(T));
                
                return;
            }
            
            battleType.Begin();

            _currBattleType = battleType;
        }

        public void ChainUpdate()
        {
            _currBattleType?.ChainUpdate();
        }

        public void ChainLateUpdate()
        {
            
        }

        #region BattleType.IListener
        void BattleType.IListener.End()
        {
            
        }
        #endregion
    }
}

