using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    public bool IsActivate { get; private set; } = false;

    public virtual void Initialize()
    {
        
    }
    
    public virtual void Activate()
    {
        IsActivate = true;
    }

    public virtual void Deactivate()
    {
        IsActivate = false;
    }
}
