using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

// 임시 스크립트
public class WorldMap : MonoBehaviour
{
    [SerializeField] 
    private Camera uiCamera = null;

    private bool _isTouch = false;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isTouch)
            return;
        
        // uiCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(uiCamera.ScreenToWorldPoint(Input.mousePosition), Vector3.down, 1);
        if (hit.collider != null)
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isTouch = true;
                
                LoadSceneManager.Instance?.LoadSceneAsync("Game");
            }
        }
    }
}
