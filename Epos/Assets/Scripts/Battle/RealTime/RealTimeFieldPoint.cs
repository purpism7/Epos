using Common;
using UnityEngine;

namespace Battle.RealTime
{
    public interface IRealTimeFieldPoint : IFieldPoint
    {
        Waypoint[] Waypoints { get; }
    }

    public class RealTimeFieldPoint : FieldPoint<RealTimeFieldPoint.Param>, IRealTimeFieldPoint
    {
        public class Param : FieldPointParam
        {
      
        }

        public Waypoint[] Waypoints { get; private set; } = null;

        protected override void Initialize(RealTimeFieldPoint.Param param)
        {
            Waypoints = GetComponentsInChildren<Waypoint>();
            foreach( var wayPoint in Waypoints)
            {
                wayPoint?.Initialize();
            }
        }
    }
}


