using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cysharp.Threading.Tasks;
using GameSystem;

public class Game : MonoBehaviour
{
    private async UniTask Awake()
    {
        // MainManager.Instance
        Debug.Log("Game Awake");
        await ResourceManager.Instance.InitializeAsync();
    }
}
