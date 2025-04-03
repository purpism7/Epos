using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField]
    private int index = 0;

    public int Index { get { return index; } }

    public void Click()
    {
        if (index != 1)
            return;
        
        LoadSceneManager.Instance?.LoadSceneAsync("Game");
    }
}
