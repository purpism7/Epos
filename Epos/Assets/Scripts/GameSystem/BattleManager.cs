using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        public void BattleBegin<T>() where T : BattleType.Battle, new();
    }
    
    public class BattleManager : Manager, IBattleManager
    {
        private Dictionary<System.Type, BattleType.Battle> _battleTypeDic = null;
        
        public override IManagerGeneric Initialize()
        {
            return this;
        }

        public void BattleBegin<T>() where T : BattleType.Battle, new()
        {
            if (_battleTypeDic == null)
            {
                _battleTypeDic = new();
                _battleTypeDic.Clear();
            }

            BattleType.Battle battle = null;
            if (!_battleTypeDic.TryGetValue(typeof(T), out battle))
            {
                battle = new T();
       
                _battleTypeDic?.TryAdd(typeof(T), battle);
            }

            if (battle == null)
            {
                Debug.Log("No Create = " + typeof(T));
                
                return;
            }
            
            battle.Begin();
        }
    }
}

