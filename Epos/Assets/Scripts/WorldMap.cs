using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using GameSystem;

// 임시 스크립트
public class WorldMap : MonoBehaviour
{
    private Entities.IGeneric _cameraIMgr = null;
    private ICameraManager _iCameraMgr = null;
    private int _selectAreaIndex = 0;
    
    private void Awake()
    {
        var cameraMgr = GetComponent<CameraManager>();

        _cameraIMgr = cameraMgr;
        _cameraIMgr?.Initialize();

        _iCameraMgr = cameraMgr;

        // _iInputMgr = GetComponent<InputManager>();
        // _iInputMgr?.Initialize();
    }

    private void Update()
    {
        if (_iCameraMgr?.MainCamera == null)
            return;

        if (_iCameraMgr.IsMove)
        {
            _selectAreaIndex = 0;

            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            var area = Area;
            if (area == null)
                return;
            
            _selectAreaIndex = area.Index;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            var area = Area;
            if (area == null)
                return;
            
            if (_selectAreaIndex == area.Index)
            {
                area.Click();
            }
            
            _selectAreaIndex = 0;
        }
    }

    private Area Area
    {
        get
        {
            var ray = _iCameraMgr.MainCamera.ScreenPointToRay(Input.mousePosition);
            var raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction);
            if (raycastHits == null)
                return null;
        
            foreach (var raycastHit in raycastHits)
            {
                var collider = raycastHit.collider;
                if (collider == null)
                    continue;

                var area = collider.GetComponentInParent<Area>();
                if (area == null)
                    continue;

                return area;
            }

            return null;
        }
    }

    private void LateUpdate()
    {
        _cameraIMgr?.ChainLateUpdate();
    }
}
