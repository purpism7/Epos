using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{ 
    public abstract class Panel<T> : Common.Component<T> where T : Common.ComponentData
    {
        // protected T _data = null;
        //
        // public virtual Panel<T> Initialize(T data = null)
        // {
        //     base.Initialize(data);
        //     
        //     _data = data;
        //
        //     return this;
        // }
    }
}


