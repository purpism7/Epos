using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

namespace GameSystem
{
    public interface IFieldManager : IManager
    {
        
    }
    
    public class FieldManager : Manager, IFieldManager
    {
        [SerializeField] 
        private Field field = null;
        
        private List<Field> _fieldList = new();
        private IField CurrIField = null; 
        
        public override IManagerGeneric Initialize()
        {
            CurrIField = field;
            CurrIField?.Initialize();
            CurrIField?.Activate();
            
            return this;
        }
        
        public override void ChainUpdate()
        {
            base.ChainUpdate();
        
            CurrIField?.ChainUpdate();
        }
    }
}

