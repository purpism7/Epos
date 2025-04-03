using UnityEngine;

namespace Creature
{
    public class EventData
    {
        public IActor IActor { get; private set; } = null;

        protected EventData(IActor iActor)
        {
            IActor = iActor;
        }
    }

    public class DamageEventData : EventData
    {
        public float Value = 0;

        public DamageEventData(IActor iActor) : base(iActor)
        {
            
        }
    }

    public interface IEventHandler<T> where T : EventData
    {
        void Add();
    }

    public class EventHandler<T>
    {
        
    }
}

