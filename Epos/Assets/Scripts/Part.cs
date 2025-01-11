using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    [SerializeField]
    private Transform rootTm = null;
    
    public bool IsActivate 
    {
        get
        {
            if (!rootTm)
                return false;
                
            return rootTm.gameObject.activeSelf;
        }
    }

    public virtual void Initialize()
    {
        
    }
    
    public virtual void Activate()
    {
        Extensions.SetActive(rootTm, true);
    }

    public virtual void Deactivate()
    {
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
    
    public virtual void Activate(T data)
    {
        _data = data;
        
        base.Activate();
    }
}
