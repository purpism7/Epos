using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
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
}
