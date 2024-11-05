using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

namespace Manager
{
    public interface IFieldManager : IManager
    {
        void MoveToTarget(Vector3 pos);
        Data.Field GetFieldData(int id);
    }
    
    public class FieldManager : MonoBehaviour, IFieldManager
    {
        [SerializeField] 
        private Parts.Field field = null;
        [SerializeField] 
        private Hero fieldHero = null;
        
        private List<Parts.Field> _fieldList = new();
        private Parts.IField CurrIField = null;

        private Dictionary<int, Data.Field> _fieldDataList = null;
        
        #region IGeneric
        public Manager.IGeneric Initialize()
        {
            field?.Initialize();
            field?.Activate();
            CurrIField = field;

            fieldHero?.Initialize();
            fieldHero?.Activate();
            
            return this;
        }
        
        void IGeneric.ChainUpdate()
        {
            CurrIField?.ChainUpdate();
            fieldHero?.ChainUpdate();
        }

        void IGeneric.ChainLateUpdate()
        {
            
        }
        #endregion
        
        #region IFieldManager
        void IFieldManager.MoveToTarget(Vector3 pos)
        {
            fieldHero?.MoveToTarget(pos);
        }

        Data.Field IFieldManager.GetFieldData(int id)
        {
            if (_fieldDataList == null)
            {
                _fieldDataList = new();
                _fieldDataList.Clear();
            }

            if (_fieldDataList.TryGetValue(id, out Data.Field fieldData))
                return fieldData;

            return null;
        }
        #endregion
    }
}

