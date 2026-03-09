using System;
using System.Collections.Generic;
using UnityEngine;

using VContainer;
using VContainer.Unity;

using Creature;

namespace Battle.RealTime
{
    public interface IWaypointController
    {
        void ChainUpdate(ICombatant targetICombatant);
        void ChainLateUpdate();
        
        Waypoint Waypoint { get; } 
        bool HasAliveMonsters { get; }
    }
    
    public class WaypointController : MonoBehaviour, IWaypointController
    {
        public static IWaypointController Create(IObjectResolver iResolver, Param param)
        {
            IWaypointController iWayPointCtr = FindFirstObjectByType<WaypointController>();
            if (iWayPointCtr == null)
            {
                var wayPointGameObj = new GameObject(nameof(WaypointController));
                iResolver?.InjectGameObject(wayPointGameObj);

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

        //[Inject] private IObjectResolver _iResolver = null;

        private Param _param = null;
        private Queue<Waypoint> _waypointQueue = null;

        public Waypoint Waypoint { get; private set; } = null;
        public bool HasAliveMonsters
        {
            get
            {
                if (Waypoint == null)
                    return false;

                return Waypoint.AliveMonsterCount > 0;
            }
        }

#if UNITY_EDITOR
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
#endif

        private IWaypointController Initialize(Param param)
        {
            _param = param;
            // Debug.Log(_iResolver);
            if (param != null && 
                !param.Waypoints.IsNullOrEmpty())
            {
                if(_waypointQueue == null)
                    _waypointQueue = new();

                _waypointQueue.Clear();

                foreach (var waypoint in param.Waypoints)
                {
                    _waypointQueue?.Enqueue(waypoint);
                }
            }

            SetWaypoint();
            
            return this;
        }
        
        void IWaypointController.ChainUpdate(ICombatant targetCombatant)
        {
            if (Waypoint == null)
                return;

            for(int i = 0; i < Waypoint.EnemyICombatantList?.Count; ++i)
            {
                var iActor = Waypoint.EnemyICombatantList[i]?.IActor;
                if (iActor == null)
                    continue;

                if (!iActor.IsActivate)
                    continue;

                iActor?.ChainUpdate();
            }
            
            if (targetCombatant == null)
                return;

            var distance = Vector3.Distance(targetCombatant.IActor.Transform.position, Waypoint.Position);
            //Debug.Log(targetICombatant.NavMeshAgent.remainingDistance);
            if (distance < 1f)
            {
                Debug.Log("Arrived");
                _param?.IListener?.Arrived();
                
                SetWaypoint();
            }
        }

        void IWaypointController.ChainLateUpdate()
        {
            if (Waypoint == null)
                return;
            
            for(int i = 0; i < Waypoint.EnemyICombatantList?.Count; ++i)
            {
                var iActor = Waypoint.EnemyICombatantList[i]?.IActor;
                if (iActor == null)
                    continue;

                if (!iActor.IsActivate)
                    continue;

                iActor.ChainLateUpdate();
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

