using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

namespace GameSystem
{
    public interface IFieldManager : IManager
    {
        IHero FieldIHero { get; }
    }
    
    public class FieldManager : Manager, IFieldManager
    {
        [SerializeField] 
        private Field field = null;
        [SerializeField] 
        private Hero fieldHero = null;
        
        private List<Field> _fieldList = new();
        private IField CurrIField = null; 
        
        // 필드 영웅은 어떻게할지이~
        private ICharacterGeneric _fieldHero = null;
        
        public override IManagerGeneric Initialize()
        {
            CurrIField = field;
            CurrIField?.Initialize();
            CurrIField?.Activate();

            fieldHero?.Initialize();
            _fieldHero = fieldHero;
            
            return this;
        }
        
        public override void ChainUpdate()
        {
            base.ChainUpdate();
        
            CurrIField?.ChainUpdate();
            _fieldHero?.ChainUpdate();
        }

        IHero IFieldManager.FieldIHero
        {
            get
            {
                return fieldHero;
            }
        }
    }
}

