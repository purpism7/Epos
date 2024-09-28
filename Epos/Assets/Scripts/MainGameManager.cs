using System;
using System.Collections;
using System.Collections.Generic;
using Common;
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
        _iMgrGenericList?.Add(gameObject.AddOrGetComponent<InputManager>()?.Initialize());
        
        _iMgrGenericList?.Add(gameObject.AddOrGetComponent<CharacterManager>()?.Initialize());
        _iMgrGenericList?.Add(gameObject.AddOrGetComponent<FieldManager>()?.Initialize());
        
        _iMgrGenericList?.Add(gameObject.AddOrGetComponent<BattleManager>()?.Initialize());
        
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

        var findIMgrGeneric = iMgrGenericList.Find(iMgrGeneric => iMgrGeneric is T);
        if (findIMgrGeneric == null)
        {
            // findIMgrGeneric = 
        }
        
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
