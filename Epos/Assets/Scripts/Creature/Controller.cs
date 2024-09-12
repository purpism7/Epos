using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public interface IController<T, V> where V : ISubject
    {
        T Initialize(V v);
        
        void ChainUpdate();
    }

    public abstract class Controller : MonoBehaviour
    {
    
    }
}

