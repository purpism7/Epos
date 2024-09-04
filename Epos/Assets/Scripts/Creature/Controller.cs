using System.Collections;
using System.Collections.Generic;
using Creature;
using UnityEngine;

public interface IController<T, V> where V : ISubject
{
    T Initialize(V v);
}

public abstract class Controller : MonoBehaviour
{
    
}
