using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Entities
{
    public interface IGeneric
    {
        IGeneric Initialize();
        void ChainUpdate();
        void ChainLateUpdate();
    }
    
    public interface IManager : IGeneric
    {
        
    }

    public abstract class Manager : MonoBehaviour
    {
        public bool IsActivate { get; private set; } = false;
    
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
