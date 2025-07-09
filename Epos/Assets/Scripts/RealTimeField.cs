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
        
        await UniTask.WaitUntil(() => UIManager.Instance.IsEndLoad);
        MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation);
    }
}