using GameSystem;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Creature;
using Entities;
using Parts;

public class RealTimeField : MonoBehaviour
{
    [SerializeField]
    private PartyLocation partyLocation = null;

    [SerializeField]
    private Transform monsterRootTm = null;
   
    private Monster[] _monsters = null;

    private async UniTask Awake()
    {
        // MainManager.Instance
        Debug.Log("RealTimeField Awake");
        await ResourceManager.Instance.InitializeAsync();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTask Start()
    {
        partyLocation?.Initialize();

        if(monsterRootTm)
            _monsters = monsterRootTm.GetComponentsInChildren<Monster>(true);

        await UniTask.WaitUntil(() => UIManager.Instance.IsEndLoad);
        MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, _monsters);
    }
}