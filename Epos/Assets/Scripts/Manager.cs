using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManager
{
    
}

public interface IManagerGeneric
{
    IManagerGeneric Initialize();
    void ChainUpdate();
    void ChainLateUpdate();
}

public abstract class Manager : MonoBehaviour, IManagerGeneric
{
    public abstract IManagerGeneric Initialize();

    public virtual void ChainUpdate()
    {
        
    }
    
    public virtual void ChainLateUpdate()
    {
        
    }
}
