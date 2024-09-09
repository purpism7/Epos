using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public interface ICameraManager
{
    
}

public class CameraManager : Manager, ICameraManager
{
    [SerializeField] 
    private Camera mainCamera = null;

    private const float DirectionForceReduceRate = 0.935f; // 감속비율
    private const float DirectionForceMin = 0.001f; // 설정치 이하일 경우 움직임을 멈춤

    // 변수 : 이동 관련
    private bool _isMove = false;
    private Vector3 _startPosition;  // 입력 시작 위치를 기억
    private Vector3 _directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기 위한 변수
   
    private float _halfHeight = 0;
    private float _height = 0;
    private float _width = 0;
    private float _dragWidth = 0;
    
    
    public override void Initialize()
    {
        
    }

    private void Start()
    {
        if (mainCamera != null)
        {
            _height = mainCamera.orthographicSize;
            _width = _height * Screen.width / Screen.height;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;
        
        var mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            StartMove(mouseWorldPos);
        }
        else if (Input.GetMouseButton(0))
        {
            if (!_isMove)
            {
                StartMove(mouseWorldPos);
                
                return;
            }
            
            _directionForce = _startPosition - mouseWorldPos;
        }
        else
        {
            _isMove = false;
        }

        ReduceDirectionForce();
        UpdateCameraPosition();
    }
    
    private void StartMove(Vector3 startPosition) 
    {
        _isMove = true;
        _startPosition = startPosition;
        _directionForce = Vector3.zero;
    }
    
    private void ReduceDirectionForce()
    {
        // 조작 중일때는 아무것도 안함
        if (_isMove)
            return;
            
        // 감속 수치 적용
        _directionForce *= DirectionForceReduceRate;
        // 작은 수치가 되면 강제로 멈춤
        if (_directionForce.magnitude < DirectionForceMin)
        {
            _directionForce = Vector3.zero;
        }
    }
    
    private void UpdateCameraPosition()
    {
        // 이동 수치가 없으면 아무것도 안함
        if (_directionForce == Vector3.zero)
            return;
        
        _halfHeight = mainCamera.orthographicSize;
        _height = _halfHeight * 2f;
        // var width= _height * mainCamera.aspect;

        _width = _halfHeight * Screen.width / Screen.height;
        
        
        
        var currentPosition = mainCamera.transform.position;
        var targetPosition = currentPosition + _directionForce;
        
        float clampX = GetClampX(targetPosition.x);
        float clampY = GetClampY(targetPosition.y);
        
        targetPosition = new Vector3(clampX, clampY, -500f);
        
        mainCamera.transform.position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime);
    }
    
    private float GetClampX(float posX)
    {
        float x = 1000f - _width;

        return Mathf.Clamp(posX, -x + 0, x + 0);
    }

    private float GetClampY(float posY)
    {
        float y = 500f - _halfHeight;

        return Mathf.Clamp(posY, -y + 0, y + 0);
    }
    
}
