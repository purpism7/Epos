using Common;
using UnityEngine;

namespace Battle.RealTime
{
    public interface IRealTimeFieldPoint : IFieldPoint
    {
        WayPoint[] WayPoints { get; }
    }

    public class RealTimeFieldPoint : FieldPoint<RealTimeFieldPoint.Param>, IRealTimeFieldPoint
    {
        public class Param : FieldPointParam
        {
      
        }

        public WayPoint[] WayPoints { get; private set; } = null;

        protected override void Initialize(RealTimeFieldPoint.Param param)
        {
            WayPoints = GetComponentsInChildren<WayPoint>();
            foreach( var wayPoint in WayPoints)
            {
                wayPoint?.Initialize();
            }
        }
    }
}


