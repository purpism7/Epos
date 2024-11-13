using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using GameSystem;

public class MainManager : Singleton<MainManager>
{
    [SerializeField]
    [Range(1f, 10f)]
    private float timeScale = 1f;
    
    private List<IGeneric> _iMgrGenericList = null;
    
    protected override void Initialize()
    {
        if (_iMgrGenericList == null)
        {
            _iMgrGenericList = new();
            _iMgrGenericList.Clear();
        }
        
        _iMgrGenericList?.Add(GetComponent<CameraManager>()?.Initialize());
        _iMgrGenericList?.Add(transform.AddOrGetComponent<InputManager>()?.Initialize());
       
        _iMgrGenericList?.Add(transform.AddOrGetComponent<Entities.FieldManager>()?.Initialize());
        
        _iMgrGenericList?.Add(new Character().Initialize());
        _iMgrGenericList?.Add(new Formation().Initialize());
        _iMgrGenericList?.Add(new BattleManager().Initialize());
    }
    
    public static T Get<T>() where T : IManager
    {
        var iMgrGenericList = Instance._iMgrGenericList;
        if (iMgrGenericList == null)
            return default;

        // var findIMgrGeneric = iMgrGenericList.Find(iMgrGeneric => iMgrGeneric is T);
        // if (findIMgrGeneric == null)
        // {
        //     // findIMgrGeneric = 
        // }
        
        foreach (var iMgrGeneric in iMgrGenericList)
        {
            if (iMgrGeneric is T)
                return (T)iMgrGeneric;
        }
        
        return default;
    }

    private void Update()
    {
#if UNITY_EDITOR
        Time.timeScale = timeScale;
#endif
        
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
