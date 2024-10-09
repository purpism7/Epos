using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

public class Part : MonoBehaviour
{
    public bool IsActivate { get; private set; } = false;

    public virtual void Initialize()
    {
        
    }
    
    public virtual void Activate()
    {
        Extension.SetActive(transform, true);
        
        IsActivate = true;
    }

    public virtual void Deactivate()
    {
        Extension.SetActive(transform, false);
        
        IsActivate = false;
    }
}

public class Part<T> : Part where T : Part<T>.BaseData
{
    public class BaseData
    {
        
    }
    
    protected T _data = null;

    public virtual void Initialize(T data)
    {
        base.Initialize();

        _data = data;
    }
}
