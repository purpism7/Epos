using System.Collections;
using UnityEngine;

using System.Collections.Generic;
using Battle.Mode;

namespace Creature
{
    public interface IStatGeneric
    {
        void Initialize(int id);
        void Activate();
        void Deactivate();

        Stat Stat { get; }
    }
    
    public interface IStat
    {
        void SetOrigin(Stat.EType eStatType, float value);

        float Get(Stat.EType eStatType);
    }
    
    public class Stat : IStatGeneric, IStat
    {
        public enum EType
        {
            None,
            
            ActionSpeed,
            
            Attack,
            MoveSpeed,
        }

        private Dictionary<EType, float> _originStatDic = new();
        private Dictionary<EType, float> _addedStatDic = new();

        #region IStatGeneric
        void IStatGeneric.Initialize(int id)
        {
            // 임시
            switch (id)
            {
                case 10001:
                {
                    SetOrigin(EType.ActionSpeed, 10f);
                    SetOrigin(EType.MoveSpeed, 8f); 
                    
                    break;
                }

                case 90001:
                {
                    // SetOrigin(EType.ActionSpeed, 10f);
                    SetOrigin(EType.MoveSpeed, 2f); 
                    
                    break;
                }
            }
        }
        
        void IStatGeneric.Activate()
        {
            
        }

        void IStatGeneric.Deactivate()
        {
            
        }

        Stat IStatGeneric.Stat
        {
            get
            {
                return this;
            }
        }
        #endregion
        
        #region IStat
        void IStat.SetOrigin(EType eType, float value)
        {
            SetOrigin(eType, value);
        }

        float IStat.Get(EType eType)
        {
            return GetOrigin(eType) + GetAdded(eType);
        }
        #endregion

        private void SetOrigin(EType eType, float value)
        {
            if (_originStatDic == null)
            {
                _originStatDic = new();
                _originStatDic.Clear();
            }

            if (_originStatDic.ContainsKey(eType))
            {
                _originStatDic[eType] = value;
            }
            else
            {
                _originStatDic.TryAdd(eType, value);
            }
        }

        private float GetOrigin(EType eType)
        {
            if (_originStatDic == null)
                return 0;
            
            if (_originStatDic.TryGetValue(eType, out float value))
                return value;

            return 0;
        }
        
        private float GetAdded(EType eType)
        {
            if (_addedStatDic == null)
                return 0;
            
            if (_addedStatDic.TryGetValue(eType, out float value))
                return value;

            return 0;
        }
    }
}

