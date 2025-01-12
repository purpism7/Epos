using System.Collections;
using UnityEngine;

using System.Collections.Generic;

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
        void Add(Stat.EType eStatType, float value);

        float Get(Stat.EType eStatType);
    }
    
    public class Stat : IStatGeneric, IStat
    {
        public enum EType
        {
            None,
            
            ActionSpeed,
            
            ActivePoint,
            PassivePoint,
            
            Attack,
            AttackRange,
            MoveSpeed,
            
            Hp,
            MaxHp,
        }

        private Dictionary<EType, float> _originStatDic = new();
        private Dictionary<EType, float> _addedStatDic = new();

        #region IStatGeneric
        void IStatGeneric.Initialize(int id)
        {
            
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
        
        void IStat.Add(EType eType, float value)
        {
            SetAdded(eType, value);
        }

        float IStat.Get(EType eType)
        {
            var curr = GetOrigin(eType) + GetAdded(eType);
            return curr;
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
                _originStatDic[eType] = value;
            else
                _originStatDic.TryAdd(eType, value);
        }
        
        private void SetAdded(EType eType, float value)
        {
            if (_addedStatDic == null)
            {
                _addedStatDic = new();
                _addedStatDic.Clear();
            }

            if (_addedStatDic.ContainsKey(eType))
                _addedStatDic[eType] += value;
            else
                _addedStatDic.TryAdd(eType, value);
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

