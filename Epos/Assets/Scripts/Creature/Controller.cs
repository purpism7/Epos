using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace Creature
{
    public interface IController<T, V> where V : ISubject
    {
        T Initialize(V v);
        void ChainUpdate();
        void Activate();
        void Deactivate();
    }

    public abstract class Controller : MonoBehaviour
    {
        public bool IsActivate { get; protected set; }

        public virtual void Activate()
        {
            IsActivate = true;
        }
        
        public virtual void Deactivate()
        {
            IsActivate = false;
        }
    }
}

