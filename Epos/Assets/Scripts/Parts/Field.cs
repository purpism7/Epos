using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using Creature;
using Entities;
using Battle;
using Battle.RealTime;

using System.IO.Compression;

namespace Parts
{
    public interface IField
    {
        void Initialize();
        void ChainUpdate();

        // FieldPoint FieldPoint { get; }
        T GetFieldPoint<T>() where T : class, IFieldPoint;
    }

    [ExecuteAlways]
    public class Field : Common.Component, IField, IFieldPointListener
    {
        [SerializeField] 
        private int id = 0;

        [Inject] private IObjectResolver _iResolver = null;

        private IFieldPoint _iFieldPoint = null;

        private Waypoint[] _waypoints = null;

        void OnEnable()
        {
#if UNITY_EDITOR
            _waypoints = GetComponentsInChildren<Waypoint>();
#endif
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            for (int i = 0; i < _waypoints?.Length - 1; i++)
            {
                if (_waypoints[i] != null)
                {
                    Gizmos.color = UnityEngine.Color.magenta;
                    Gizmos.DrawSphere(_waypoints[i].Position, 1f);

                    if (_waypoints[i + 1] != null)
                    {
                        Gizmos.color = UnityEngine.Color.magenta;
                        Gizmos.DrawLine(_waypoints[i].Position, _waypoints[i + 1].Position);
                    }
                }
            }
        }
#endif

        #region IField
        void IField.ChainUpdate()
        {
            //if (fieldPoints == null)
            //    return;
    
            //foreach (var fieldPoint in fieldPoints)
            //{
            //    fieldPoint?.ChainUpdate();
            //}
        }
    
        // FieldPoint IField.FieldPoint
        // {
        //     get
        //     {
        //         return fieldPoints[0];
        //     }
        // }
        #endregion
    
        public override void Initialize()
        {
            _iFieldPoint = GetComponentInChildren<IFieldPoint>();
            _iResolver?.Inject(_iFieldPoint);
            //_iFieldPoints = GetComponentsInChildren<IFieldPoint>();

            //foreach (var iFieldPoint in _iFieldPoints)
            {
                switch (_iFieldPoint)
                {
                    case IRealTimeFieldPoint iRealTimeFieldPoint:
                        {
                            var fieldPointParam = new RealTimeFieldPoint.Param();
                            //.WithIFieldPointListener(this);

                            fieldPointParam.WithIFieldPointListener(this);
                            _iFieldPoint?.Initialize(fieldPointParam);

                            break;
                        }

                    case TurnBasedFieldPoint fieldPoint:
                        {
                            break;
                        }
                }
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

        T IField.GetFieldPoint<T>() where T : class
        {
            return _iFieldPoint as T;
        }
        //T IField.GetFieldPoint<T>()
        //{
        //    return _iFieldPoint as T;
        //}

        private void ActivateFieldPoints()
        {
            //if (fieldPoints == null)
            //    return;
    
            //foreach (var fieldPoint in fieldPoints)
            //{
            //    fieldPoint?.Activate();
            //}
        }
        
        private void DeactivateFieldPoints()
        {
            //if (fieldPoints == null)
            //    return;
    
            //foreach (var fieldPoint in fieldPoints)
            //{
            //    fieldPoint?.Deactivate();
            //}
        }
        
        #region FieldPoint.IListener

        //void Battle.FieldPoint.IListener.Encounter(int fieldPointId, IActor iActor)
        //{
        //    if (iActor == null)
        //        return;
            
        //    // iActor.IActCtr?.Idle();

        //    // MainManager.Get<IFieldManager>()?.Deactivate();
        //}
        #endregion
    }
}
