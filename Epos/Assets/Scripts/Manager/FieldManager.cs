using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;
using Parts;
using Field = Data.Field;

namespace Manager
{
    public interface IFieldManager : IManager
    {
        void MoveToTarget(Vector3 pos);

        void Activate();
        void Deactivate();
        
        Hero FieldHero { get; }
        // Data.Field GetFieldData(int id);
    }
    
    public class FieldManager : MonoBehaviour, IFieldManager
    {
        [SerializeField] 
        private Parts.Field field = null;
        [SerializeField] 
        private Hero fieldHero = null;
        [SerializeField] 
        private FieldIndicator fieldIndicator = null;
        
        private List<Parts.Field> _fieldList = new();
        private Parts.IField CurrIField = null;

        // 임시.
        private List<Data.Field> _fieldDataList = null;

        public bool IsActivate { get; private set; } = false;
        
        #region IGeneric
        public Manager.IGeneric Initialize()
        {
            field?.Initialize();
            field?.Activate();
            CurrIField = field;

            fieldHero?.Initialize();
            fieldHero?.Activate();

            _fieldDataList = new List<Field>();
            _fieldDataList?.Clear();
            
            _fieldDataList?.Add(new Field(1));
            _fieldDataList?.Add(new Field(2));
            
            fieldIndicator?.Deactivate();
            
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

        public void Activate()
        {
            fieldHero?.Activate();
        }

        public void Deactivate()
        {
            fieldIndicator?.Deactivate();
        }
        
        #region IFieldManager
        void IFieldManager.MoveToTarget(Vector3 pos)
        {
            if (!fieldHero.IsActivate)
                return;
            
            fieldHero?.MoveToTarget(pos,
                () =>
                {
                    fieldIndicator?.Deactivate();
                });
            
            fieldIndicator?.Activate(
                new FieldIndicator.Data
                {
                    TargetPos = pos,
                });
        }
        
        Hero IFieldManager.FieldHero
        {
            get
            {
                return fieldHero;
            }
        }

        // Data conatainer 로 변경 할 것.
        // Data.Field IFieldManager.GetFieldData(int fieldPointId)
        // {
        //     if (_fieldDataList == null)
        //         return null;
        //     
        //     foreach (var fieldData in _fieldDataList)
        //     {
        //         if (fieldData == null)
        //             continue;
        //
        //         if (fieldData.FieldPointId == fieldPointId)
        //             return fieldData;
        //     }
        //
        //     // if (_fieldDataList.TryGetValue(id, out Data.Field fieldData))
        //     //     return fieldData;
        //
        //     return null;
        // }
        #endregion
    }
}

