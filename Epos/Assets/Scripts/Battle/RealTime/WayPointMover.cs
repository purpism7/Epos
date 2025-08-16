using System;
using System.Collections.Generic;
using UnityEngine;

using Component = Common.Component;

namespace Battle.RealTime
{
    public interface IWayPointMover
    {
        void ChainUpdate(Transform targetTm);
        
        WayPoint WayPoint { get; } 
    }
    
    public class WayPointMover : MonoBehaviour, IWayPointMover
    {
        public static IWayPointMover Create(Param param)
        {
            IWayPointMover iWayPointMover = FindFirstObjectByType<WayPointMover>();
            if (iWayPointMover == null)
            {
                var wayPointGameObj = new GameObject(nameof(WayPointMover));
                iWayPointMover = wayPointGameObj.transform.AddOrGetComponent<WayPointMover>()
                    .Initialize(param);
            }

            return iWayPointMover;
        }
        
        public class Param
        {
            public WayPoint[] WayPoints { get; private set; } = null;

            public Param WithWayPoints(WayPoint[] wayPoints)
            {
                WayPoints = wayPoints;
                return this;
            }
        }

        private Param _param = null;
        private Queue<WayPoint> _wayPointQueue = null;

        public WayPoint WayPoint { get; private set; } = null;

        private void OnDrawGizmos()
        {
            var wayPoints = _param?.WayPoints;
            if (wayPoints == null)
                return;
            
            for (int i = 0; i < wayPoints.Length - 1; i++)
            {
                if (wayPoints[i] != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(wayPoints[i].Position, 1f);
                    
                    if(wayPoints[i + 1] != null)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(wayPoints[i].Position, wayPoints[i + 1].Position);
                    }
                }
            }
        }

        private IWayPointMover Initialize(Param param)
        {
            _param = param;
            
            if (param != null && 
                !param.WayPoints.IsNullOrEmpty())
            {
                if(_wayPointQueue == null)
                    _wayPointQueue = new();

                _wayPointQueue.Clear();

                foreach (var wayPoint in param.WayPoints)
                {
                    _wayPointQueue?.Enqueue(wayPoint);
                }
            }

            SetWayPoint();
            
            return this;
        }
        
        void IWayPointMover.ChainUpdate(Transform targetTm)
        {
            if (!targetTm)
                return;

            var distance = Vector3.Distance(targetTm.position, WayPoint.Position);
            if (distance <= 1f)
            {
                Debug.Log("Arrived");
            }
            // WayPoint.Position
        }

        private void SetWayPoint()
        {
            if (_wayPointQueue == null)
                return;
            
            if (_wayPointQueue.TryDequeue(out WayPoint wayPoint))
                WayPoint = wayPoint;
        }
    }
}

