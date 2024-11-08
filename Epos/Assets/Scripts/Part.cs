using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    [SerializeField]
    private Transform rootTm = null;
    
    public bool IsActivate { get; private set; } = false;

    public virtual void Initialize()
    {
        
    }
    
    public virtual void Activate()
    {
        IsActivate = true;
        
        Extensions.SetActive(rootTm, true);
    }

    public virtual void Deactivate()
    {
        IsActivate = false;
        
        Extensions.SetActive(rootTm, false);
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
