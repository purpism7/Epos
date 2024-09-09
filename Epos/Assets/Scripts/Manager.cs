using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManagerGeneric
{
    void Initialize();
}

public abstract class Manager : MonoBehaviour, IManagerGeneric
{
    public abstract void Initialize();
}
