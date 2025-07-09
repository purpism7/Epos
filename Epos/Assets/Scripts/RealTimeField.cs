using Creature;
using Cysharp.Threading.Tasks;
using Entities;
using GameSystem;
using System.Collections.Generic;
using UnityEngine;

public class RealTimeField : MonoBehaviour
{
    [SerializeField]
    private Transform heroRootTm = null;
    [SerializeField]
    private GameSystem.Grid heroGrid = null;

    private async UniTask Awake()
    {
        // MainManager.Instance
        Debug.Log("RealTimeField Awake");
        await ResourceManager.Instance.InitializeAsync();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTask Start()
    {
        await UniTask.WaitUntil(() => UIManager.Instance.IsEndLoad);
        MainManager.Get<IBattleManager>()?.BeginRealTime(heroGrid);
    }
}