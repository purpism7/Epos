using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

using Cysharp.Threading.Tasks;

using DG.Tweening;
using Vector3 = UnityEngine.Vector3;

using Entities;

namespace GameSystem
{
    public interface ICameraManager : Entities.IManager
    {
        Camera MainCamera { get; }
        bool IsMove { get; }

        void ZoomIn(Vector3 targetPos, Action endAction);
        void ZoomOut(Action endAction);
    }
    
    public class CameraManager : MonoBehaviour, ICameraManager
    {
        [SerializeField] 
        [Range(0.1f, 5f)]
        private float zoomInOutDuration = 1f;
        
        [SerializeField] 
        private Camera mainCamera = null;
        [SerializeField] 
        private CinemachineVirtualCamera virtualCamera = null;

        private const float DefaultOrthographicSize = 32f;
        private const float DefaultZPos = -200f;
            
        #region Drag
        private const float DirectionForceReduceRate = 0.935f; // 감속비율
        private const float DirectionForceMin = 0.001f; // 설정치 이하일 경우 움직임을 멈춤

        // 변수 : 이동 관련
        private Vector3 _startPosition;  // 입력 시작 위치를 기억
        private Vector3 _directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기 위한 변수
        #endregion

        private Creature.Hero _fieldHero = null;
        private bool _return = true;
        private float _returnTime = 0;
        
        public Camera MainCamera { get { return mainCamera; } }
        public bool IsMove { get; private set; }
        
        public Entities.IGeneric Initialize()
        {
            _return = true;
            
            return this;
        }
        
        void Entities.IGeneric.ChainUpdate()
        {
            
        }

        public void ChainLateUpdate()
        {
            if (mainCamera == null)
                return;

            if (virtualCamera == null)
                return;

            FieldChainLateUpdate();
        }

        private void FieldChainLateUpdate()
        {
            if (!_fieldHero)
            {
                _fieldHero = MainManager.Get<IFieldManager>()?.FieldHero;

                return;
            }

            if (!_fieldHero.IsActivate)
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

                    _return = false;
                    _returnTime = 0;
                    
                    return;
                }
                
                _directionForce = _startPosition - mouseWorldPos;
            }
            else
            {
                if (IsMove)
                    _return = true;

                if (_return)
                    _returnTime += Time.deltaTime;
                
                IsMove = false;

                if (CameraReturnToCharacter())
                    return;
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
            if (_directionForce == Vector3.zero)
                return;
            
            var currentPos = mainCamera.transform.position;
            var targetPos = currentPos + _directionForce;

            mainCamera.transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 2f);
        }

        private bool CameraReturnToCharacter()
        {
            if (mainCamera == null)
                return false;
            
            if (IsMove ||
                _returnTime < 1f)
                return false;
            
            var pos = _fieldHero.Transform.position;
            pos.z = -10f;
                    
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, pos, Time.deltaTime * 2f);

            return true;
        }
        
        #region Zoom In / Out
        void ICameraManager.ZoomIn(Vector3 targetPos, Action endAction)
        {
            ZoomInAsync(targetPos, endAction).Forget();
        }

        private async UniTask ZoomInAsync(Vector3 targetPos, Action endAction)
        { 
            var duration = zoomInOutDuration;
            targetPos.z = DefaultZPos;
            
            DOTween.To(() => virtualCamera.m_Lens.OrthographicSize,
                orthographicSize => virtualCamera.m_Lens.OrthographicSize = orthographicSize, 20f, duration);
            await DOTween.To(() => mainCamera.transform.position,
                position => mainCamera.transform.position = position, targetPos, duration).SetEase(Ease.OutCirc);

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            endAction?.Invoke();
        }

        void ICameraManager.ZoomOut(Action endAction)
        {
            ZoomOutAsync(endAction).Forget();
        }
        
        private async UniTask ZoomOutAsync(Action endAction)
        { 
            var duration = zoomInOutDuration;
            
            await DOTween.To(() => virtualCamera.m_Lens.OrthographicSize,
                orthographicSize => virtualCamera.m_Lens.OrthographicSize = orthographicSize, DefaultOrthographicSize, duration).SetEase(Ease.Linear);

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            endAction?.Invoke();
        }
        #endregion
    }
}

