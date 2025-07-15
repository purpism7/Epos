using GameSystem;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Creature;
using Entities;
using Parts;
using Battle;

public class RealTimeField : MonoBehaviour
{
    [SerializeField]
    private PartyLocation partyLocation = null;
    [SerializeField]
    private WayPoint[] wayPoints = null;

    private async UniTask Awake()
    {
        // MainManager.Instance

        await ResourceManager.Instance.InitializeAsync();

        foreach (var wayPoint in wayPoints)
        {
            wayPoint?.Initialize();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTask Start()
    {
        partyLocation?.Initialize();

        //if (monsterRootTm)
        //    _monsters = monsterRootTm.GetComponentsInChildren<Monster>();

        await UniTask.WaitUntil(() => UIManager.Instance.IsEndLoad);
        MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
    }
}