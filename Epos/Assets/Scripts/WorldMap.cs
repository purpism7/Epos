using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

// 임시 스크립트
public class WorldMap : MonoBehaviour
{
    public void OnClickArea(int areaId)
    {
        if (areaId == 0)
        {
            LoadSceneManager.Instance?.LoadSceneAsync("Game");
        }
    }
}
