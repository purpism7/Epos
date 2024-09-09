using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : Singleton<MainGameManager>
{

    private List<IManagerGeneric> _iMgrGenericList = null;
    protected override void Initialize()
    {
        
        
        GetComponent<CameraManager>();
        
        Debug.Log("MainGameManager");
    }
}
