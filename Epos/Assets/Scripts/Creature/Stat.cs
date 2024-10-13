using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    
    public class Stat : IStatGeneric, IStat
    {
        public enum EType
        {
            None,
            
            ActSpeed,
            
            Attack,
            MoveSpeed,
        }

        private Dictionary<EType, float> _originStatDic = new();
        private Dictionary<EType, float> _addStatDic = new();

        #region IStatGeneric
        void IStatGeneric.Initialize(int id)
        {
            switch (id)
            {
                case 1:
                {
                    SetOrigin(EType.ActSpeed, 10f);
                    
                    break;
                }
            }
        }
        
        // 데이터화가 되기 전, 임시 코드.
        public void SetOriginActSpeed(float actSpeed)
        {
            SetOrigin(EType.ActSpeed, actSpeed);
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
    }
}

