using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{ 
    public abstract class Panel<T> : UI.Component<T> where T : UI.Component.Data
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


