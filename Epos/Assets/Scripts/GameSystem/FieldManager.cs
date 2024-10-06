using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

namespace GameSystem
{
    public interface IFieldManager : IManager
    {
        void MoveToTarget(Vector3 pos);
    }
    
    public class FieldManager : Manager, IFieldManager
    {
        [SerializeField] 
        private Parts.Field field = null;
        [SerializeField] 
        private Hero fieldHero = null;
        
        private List<Parts.Field> _fieldList = new();
        private Parts.IField CurrIField = null; 
        
        public override IManagerGeneric Initialize()
        {
            field?.Initialize();
            field?.Activate();
            CurrIField = field;

            fieldHero?.Initialize();
            fieldHero?.Activate();
            
            return this;
        }
        
        public override void ChainUpdate()
        {
            base.ChainUpdate();
        
            CurrIField?.ChainUpdate();
            fieldHero?.ChainUpdate();
        }
        
        void IFieldManager.MoveToTarget(Vector3 pos)
        {
            fieldHero?.MoveToTarget(pos);
            
            MainGameManager.Get<ICameraManager>()?.ZoomIn(CurrIField.FieldPoint.PointTm.position);
            
        }
    }
}

