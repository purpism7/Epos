using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace GameSystem
{
    public interface ICameraManager : IManager
    {
        Camera MainCamera { get; }
        bool IsMove { get; }
    }
    
    public class CameraManager : Manager, ICameraManager
    {
        [SerializeField] 
        private Camera mainCamera = null;
        [SerializeField] 
        private CinemachineVirtualCamera virtualCamera = null;

        private const float DirectionForceReduceRate = 0.935f; // 감속비율
        private const float DirectionForceMin = 0.001f; // 설정치 이하일 경우 움직임을 멈춤

        // 변수 : 이동 관련
        private Vector3 _startPosition;  // 입력 시작 위치를 기억
        private Vector3 _directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기 위한 변수

        public Camera MainCamera { get { return mainCamera; } }
        public bool IsMove { get; private set; }

        public override IManagerGeneric Initialize()
        {
            
            
            return this;
        }

        public override void ChainLateUpdate()
        {
            base.ChainLateUpdate();
            
            if (mainCamera == null)
                return;

            if (virtualCamera == null)
                return;
            
            var mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                StartMove(mouseWorldPos);
            }
            else if (Input.GetMouseButton(0))
            {
                if (!CheckDrag(_startPosition, mouseWorldPos))
                    return;
                
                if (!IsMove)
                {
                    IsMove = true;

                    return;
                }
                
                _directionForce = _startPosition - mouseWorldPos;
            }
            else
            {
                IsMove = false;
            }

            ReduceDirectionForce();
            UpdateCameraPosition();
        }
        
        bool CheckDrag(Vector3 startPos, Vector3 currPos)
        {
            return (currPos - startPos).sqrMagnitude >= 0.01f;
        }
        
        private void StartMove(Vector3 startPosition) 
        {
            // IsMove = true;
            _startPosition = startPosition;
            _directionForce = Vector3.zero;
        }
        
        private void ReduceDirectionForce()
        {
            // 조작 중일때는 아무것도 안함
            if (IsMove)
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
            
            var currentPosition = mainCamera.transform.position;
            var targetPosition = currentPosition + _directionForce;

            mainCamera.transform.position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime);
        }

        #region Zoom In / Out
        private void UpdateZoomIn()
        {
            if (virtualCamera == null)
                return;
            
            virtualCamera.m_Lens.OrthographicSize =  Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, 20f, Time.deltaTime);
        }
        #endregion
    }
}

