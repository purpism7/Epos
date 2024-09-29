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
        private Field field = null;
        [SerializeField] 
        private Hero fieldHero = null;
        
        private List<Field> _fieldList = new();
        private IField CurrIField = null; 
        
        public override IManagerGeneric Initialize()
        {
            CurrIField = field;
            CurrIField?.Initialize();
            CurrIField?.Activate();

            fieldHero?.Initialize();
            
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
            
            MainGameManager.Get<IBattleManager>()?.Begin<Battle.Field>();
        }
    }
}

