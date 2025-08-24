using System;
using System.Collections.Generic;
using UnityEngine;

using Component = Common.Component;

namespace Battle.RealTime
{
    public interface IWaypointController
    {
        void ChainUpdate(Transform targetTm);
        void ChainLateUpdate();
        
        Waypoint Waypoint { get; } 
    }
    
    public class WaypointController : MonoBehaviour, IWaypointController
    {
        public static IWaypointController Create(Param param)
        {
            IWaypointController iWayPointCtr = FindFirstObjectByType<WaypointController>();
            if (iWayPointCtr == null)
            {
                var wayPointGameObj = new GameObject(nameof(WaypointController));
                iWayPointCtr = wayPointGameObj.transform.AddOrGetComponent<WaypointController>()
                    .Initialize(param);
            }

            return iWayPointCtr;
        }
        
        public class Param
        {
            public IListener IListener { get; private set; } = null;
            public Waypoint[] Waypoints { get; private set; } = null;

            public Param WithIListener(IListener iListener)
            {
                IListener = iListener;
                return this;
            }
            
            public Param WithWaypoints(Waypoint[] waypoints)
            {
                Waypoints = waypoints;
                return this;
            }
        }

        public interface IListener
        {
            void Arrived();
        }

        private Param _param = null;
        private Queue<Waypoint> _waypointQueue = null;

        public Waypoint Waypoint { get; private set; } = null;

        private void OnDrawGizmos()
        {
            var wayPoints = _param?.Waypoints;
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

        private IWaypointController Initialize(Param param)
        {
            _param = param;
            
            if (param != null && 
                !param.Waypoints.IsNullOrEmpty())
            {
                if(_waypointQueue == null)
                    _waypointQueue = new();

                _waypointQueue.Clear();

                foreach (var wayPoint in param.Waypoints)
                {
                    _waypointQueue?.Enqueue(wayPoint);
                }
            }

            SetWaypoint();
            
            return this;
        }
        
        void IWaypointController.ChainUpdate(Transform targetTm)
        {
            if (Waypoint == null)
                return;

            for(int i = 0; i < Waypoint.EnemyICombatantList?.Count; ++i)
            {
                var enemyICombatant = Waypoint.EnemyICombatantList[i];
                if (enemyICombatant == null)
                    continue;

                if (!enemyICombatant.IsActivate)
                    continue;

                enemyICombatant.IActCtr?.ChainUpdate();
            }
            
            if (!targetTm)
                return;

            var distance = Vector3.Distance(targetTm.position, Waypoint.Position);
            if (distance <= 1f)
            {
                Debug.Log("Arrived");
                _param?.IListener?.Arrived();
                
                SetWaypoint();
            }
        }

        void IWaypointController.ChainLateUpdate()
        {
            for(int i = 0; i < Waypoint.EnemyICombatantList?.Count; ++i)
            {
                var enemyICombatant = Waypoint.EnemyICombatantList[i];
                if (enemyICombatant == null)
                    continue;

                if (!enemyICombatant.IsActivate)
                    continue;

                enemyICombatant.ChainLateUpdate();
            }
        }

        private void SetWaypoint()
        {
            Waypoint = null;

            if (_waypointQueue == null)
                return;
            
            if (_waypointQueue.TryDequeue(out Waypoint waypoint))
                Waypoint = waypoint;
        }
    }
}

