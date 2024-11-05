using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;
using GameSystem;
using Manager;

namespace Parts
{
    public interface IField
    {
        void ChainUpdate();
        
        FieldPoint FieldPoint { get; }
    }
    
    public class Field : Part, IField, FieldPoint.IListener
    {
        [SerializeField] 
        private int id = 0;
        [SerializeField] 
        private FieldPoint[] fieldPoints = null;
    
        #region IField
        void IField.ChainUpdate()
        {
            if (fieldPoints == null)
                return;
    
            foreach (var fieldPoint in fieldPoints)
            {
                fieldPoint?.ChainUpdate();
            }
        }
    
        FieldPoint IField.FieldPoint
        {
            get
            {
                return fieldPoints[0];
            }
        }
        #endregion
    
        public override void Initialize()
        {
            if (fieldPoints == null)
                return;
    
            foreach (var fieldPoint in fieldPoints)
            {
                fieldPoint?.Initialize(
                    new FieldPoint.Data
                    {
                        IListener = this,
                    });
            }
        }

        public override void Activate()
        {
            base.Activate();
            
            ActivateFieldPoints();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            DeactivateFieldPoints();
        }

        private void ActivateFieldPoints()
        {
            if (fieldPoints == null)
                return;
    
            foreach (var fieldPoint in fieldPoints)
            {
                fieldPoint?.Activate();
            }
        }
        
        private void DeactivateFieldPoints()
        {
            if (fieldPoints == null)
                return;
    
            foreach (var fieldPoint in fieldPoints)
            {
                fieldPoint?.Deactivate();
            }
        }
        
        #region FieldPoint.IListener

        void FieldPoint.IListener.Encounter(int fieldPointId, IActor iActor)
        {
            if (iActor == null)
                return;
            
            iActor.IActCtr?.Idle();
            
            // MainManager.Get<IFieldManager>().GetFieldData()
        }
        #endregion
    }
}
