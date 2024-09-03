using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;


public interface ICharacterGeneric
{
    void Initialize();
}

public abstract class Character : MonoBehaviour, ICharacterGeneric
{
    #region Inspector
    [SerializeField] 
    private int id = 0;
    #endregion

    private SkeletonAnimation _skeletonAnimation = null;

    // 일단 Awake 사용 
    private void Awake()
    {
        Initialize();
    }

    #region ICharacterGeneric
    public virtual void Initialize()
    {
        _skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
    }
    #endregion
}
