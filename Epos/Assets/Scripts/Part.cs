using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Part<T> : Common.Component<T> where T : Common.Param
{
    // protected T _data = null;

    // public virtual Part<T> Initialize(T data)
    // {
    //     base.Initialize();
    //
    //     _data = data;
    //
    //     return this;
    // }
    
    // public virtual void Activate(T data)
    // {
    //     _data = data;
    //     
    //     base.Activate();
    // }
}
