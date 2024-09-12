using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameSystem;

public class MainGameManager : Singleton<MainGameManager>
{
    private List<IManagerGeneric> _iMgrGenericList = null;
    
    protected override void Initialize()
    {
        _iMgrGenericList = new();
        _iMgrGenericList.Clear();
        
        _iMgrGenericList?.Add(GetComponent<CameraManager>()?.Initialize());
        _iMgrGenericList?.Add(Common.Extension.AddOrGetComponent<InputManager>(gameObject)?.Initialize());
        
        Debug.Log("MainGameManager");
    }

    private void Awake()
    {
        Initialize();
    }

    public static T Get<T>() where T : IManager
    {
        var iMgrGenericList = Instance._iMgrGenericList;
        if (iMgrGenericList == null)
            return default;
        
        foreach (var iMgrGeneric in iMgrGenericList)
        {
            if (iMgrGeneric is T)
                return (T)iMgrGeneric;
        }

        return default;
    }

    private void Update()
    {
        if (_iMgrGenericList != null)
        {
            foreach (var iMgrGeneric in _iMgrGenericList)
            {
                iMgrGeneric?.ChainUpdate();
            }
        }
    }

    private void LateUpdate()
    {
        if (_iMgrGenericList != null)
        {
            foreach (var iMgrGeneric in _iMgrGenericList)
            {
                iMgrGeneric?.ChainLateUpdate();
            }
        }
    }
}
