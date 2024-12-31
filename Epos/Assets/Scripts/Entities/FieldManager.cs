using System.Collections;
using System.Collections.Generic;
using Creator;
using UnityEngine;

using Creature;
using Parts;

namespace Entities
{
    public interface IFieldManager : IManager
    {
        void MoveToTarget(Vector3 pos);

        void Activate();
        void Deactivate();
        
        Hero FieldHero { get; }
    }
    
    public class FieldManager : Manager, IFieldManager
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
        private List<Datas.Field> _fieldDataList = null;
        
        #region IGeneric
        public Entities.IGeneric Initialize()
        {
            field?.Initialize();
            field?.Activate();
            CurrIField = field;

            fieldHero?.Initialize();

            _fieldDataList = new();
            _fieldDataList?.Clear();
            
            _fieldDataList?.Add(new Datas.Field(1));
            _fieldDataList?.Add(new Datas.Field(2));
            
            fieldIndicator?.Deactivate();
            
            Activate();

           
            
            return this;
        }
        
        void IGeneric.ChainUpdate()
        {
            if (!IsActivate)
                return;
            
            CurrIField?.ChainUpdate();
            fieldHero?.ChainUpdate();
        }

        void IGeneric.ChainLateUpdate()
        {
            
        }
        
        void FixedUpdate()
        {
            fieldHero?.ChainFixedUpdate();
        }
        #endregion

        public override void Activate()
        {
            base.Activate();
            
            fieldHero?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
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

