using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using VContainer;

using Entities;
using GameSystem.Event;
using UnityEngine.Rendering.Universal;
using Vector3 = UnityEngine.Vector3;

namespace GameSystem
{
    public interface ICameraManager
    {
        Camera MainCamera { get; }
        bool IsMove { get; }

        //void MoveToTarget(Vector3 targetPosition);
        UniTask InitializeAsync(Camera mainCamera, CinemachineVirtualCamera virtualCamera);
        
        void SetTargetTr(Transform targetTm, Vector3 offsetPosition);
        
        void FocusOnTarget(Action endAction, float targetSize = 20f, Vector3? offsetPosition = null);
        void ClearFocus(Action endAction = null);
    }
    
    public class CameraManager : Manager, ICameraManager
    {
        [SerializeField] 
        // [Range(0.1f, 5f)]
        private float _zoomInOutDuration = 0.5f;
        
        // [SerializeField] private Camera mainCamera = null;
        // [SerializeField] private CinemachineVirtualCamera virtualCamera = null;

        private float _defaultOrthographicSize = 20f;
        private Vector3 _defaultCameraPosition = Vector3.zero;
        // private const float DefaultZPos = -200f;
            
        private CinemachineVirtualCamera _virtualCamera = null;
        
        #region Drag
        private const float DirectionForceReduceRate = 0.935f; // 감속비율
        private const float DirectionForceMin = 0.001f; // 설정치 이하일 경우 움직임을 멈춤

        // 변수 : 이동 관련
        private Vector3 _startPosition;  // 입력 시작 위치를 기억
        private Vector3 _directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기 위한 변수
        #endregion

        //private float _returnTime = 0;

        //private Vector3? _targetPosition = null;
        private Transform _targetTm = null;
        private Vector3 _targetOffsetPosition = Vector3.zero;
        private Vector3 _shakeOffset = Vector3.zero;
        private Tween _shakeTween;
        private Tween _focusZoomTween;
        private Tween _focusMoveTween;
        private bool _hasManualFocusPosition = false;

        public Camera MainCamera { get; private set; } = null;
        public bool IsMove { get; private set; }

        private void OnEnable()
        {
            GameSystem.Event.EventHandler.Add<SkillImpactEventData>(OnSkillImpact);
        }

        private void OnDisable()
        {
            GameSystem.Event.EventHandler.Remove<SkillImpactEventData>(OnSkillImpact);
        }

        public UniTask InitializeAsync(Camera mainCamera, CinemachineVirtualCamera virtualCamera)
        {
            MainCamera = mainCamera;
            _virtualCamera = virtualCamera;
            _focusZoomTween?.Kill();
            _focusMoveTween?.Kill();

            if (_virtualCamera != null)
                _defaultOrthographicSize = _virtualCamera.m_Lens.OrthographicSize;

            if (MainCamera != null)
                _defaultCameraPosition = MainCamera.transform.position;

            _hasManualFocusPosition = false;

            return UniTask.CompletedTask;
        }

        private void OnSkillImpact(SkillImpactEventData eventData)
        {
            var skillData = eventData?.ISkill?.SkillData;
            if (skillData == null || !skillData.ShakeCamera)
                return;

            Shake();
        }

        public void Shake(float duration = 0.3f, float strength = 1f)
        {
            if (MainCamera == null)
                return;

            // 1. 기존에 진행 중인 쉐이크가 있다면 강제로 종료 (중복 실행 충돌 방지)
            if (_shakeTween != null && _shakeTween.IsActive())
            {
                _shakeTween.Kill();
            }

            // 2. 오프셋 초기화
            _shakeOffset = Vector3.zero;

            // 3. 쉐이크 실행 및 Tween 참조 저장
            _shakeTween = DOTween.Shake(
                    () => _shakeOffset, 
                    x => _shakeOffset = x, 
                    duration, 
                    strength, 
                    30, 
                    90f, 
                    false, 
                    true, 
                    ShakeRandomnessMode.Full
                )
                .SetUpdate(true)
                // 4. 안전장치: 쉐이크가 완전히 끝났을 때 오프셋을 0으로 원복
                .OnKill(() => _shakeOffset = Vector3.zero); 
        }

        private void LateUpdate()
        {
            if (MainCamera == null)
                return;

            if (_virtualCamera == null)
                return;
            
            FieldChainLateUpdate();
        }

        // public void ChainLateUpdate()
        // {
        //     if (mainCamera == null)
        //         return;
        //
        //     if (virtualCamera == null)
        //         return;
        //     
        //     FieldChainLateUpdate();
        // }

        private void FieldChainLateUpdate()
        {
            //if (!_fieldHero)
            //{
            //    _fieldHero = MainManager.Get<IFieldManager>()?.FieldHero;

            //    return;
            //}

            //if (!_fieldHero.IsActivate)
            //    return;
            //var pointerPos = Input.mousePosition;
            //if (float.IsInfinity(pointerPos.x) || float.IsInfinity(pointerPos.y))
            //    return; //

            //var mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPos);
            //if (Input.GetMouseButtonDown(0))
            //{
            //    StartMove(mouseWorldPos);
            //}
            //else if (Input.GetMouseButton(0))
            //{
            //    if (!CheckDrag(_startPosition, mouseWorldPos))
            //        return;
                
            //    if (!IsMove)
            //    {
            //        IsMove = true;

            //        _return = false;
            //        _returnTime = 0;
                    
            //        return;
            //    }
                
            //    _directionForce = _startPosition - mouseWorldPos;
            //}
            //else
            //{
            //    if (IsMove)
            //        _return = true;

            //    if (_return)
            //        _returnTime += Time.deltaTime;
                
            //    IsMove = false;

            //    if (MoveToTarget())
            //        return;
            //}

            ReduceDirectionForce();
            UpdateCameraPosition();
        }
        
        bool CheckDrag(Vector3 startPos, Vector3 currPos)
        {
            return (currPos - startPos).sqrMagnitude >= 0.01f;
        }
        
        // private void StartMove(Vector3 startPosition) 
        // {
        //     _startPosition = startPosition;
        //     _directionForce = Vector3.zero;
        // }
        
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
        
        // private void UpdateCameraPosition()
        // {
        //     if (_directionForce == Vector3.zero)
        //         return;
        //     
        //     var currentPos = mainCamera.transform.position;
        //     var targetPos = currentPos + _directionForce;
        //
        //     mainCamera.transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 2f);
        // }
        
        private void UpdateCameraPosition()
        {
            if (!_targetTm)
                return;
        
            var currentPos = MainCamera.transform.position;
            var targetPos = _targetTm.position + _targetOffsetPosition;
            targetPos.z = -100f;
            
            MainCamera.transform.position = Vector3.Lerp(currentPos, targetPos, Time.unscaledDeltaTime) + _shakeOffset;
    
            // ReturnDistance = Vector3.Distance(currentPos, targetPos);
        }

        //private bool MoveToTarget()
        //{
        //    if (mainCamera == null)
        //        return false;

        //    if (_targetPosition == null)
        //        return false;
            
        //    if (IsMove ||
        //        _returnTime < 1f)
        //        return false;

        //    // var navMeshTm = _fieldHero?.NavMeshAgent?.transform;
        //    // if (!navMeshTm)
        //    //     return false;

        //    var targetPosition = _targetPosition.Value;
        //    targetPosition.z = -100f;
            
        //    mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime);

        //    return true;
        //}

        //public void MoveToTarget(Vector3 targetPosition)
        //{
        //    _targetPosition = targetPosition;
        //}
        
        public void SetTargetTr(Transform targetTm, Vector3 offsetPosition)
        {
            _targetTm = targetTm;
            _targetOffsetPosition = offsetPosition;
        }
        
        #region Zoom
        void ICameraManager.FocusOnTarget(Action endAction, float targetSize, Vector3? targetPosition)
        {
            FocusOnTargetAsync(endAction, targetSize, targetPosition).Forget();
        }

        private async UniTask FocusOnTargetAsync(Action endAction, float targetSize, Vector3? targetPosition = null)
        { 
            var duration = _zoomInOutDuration;
           
            var tasks = new List<UniTask>();

            // 1. Orthographic Size (줌) 트윈
            _focusZoomTween?.Kill();
            _focusZoomTween = DOTween.To(() => _virtualCamera.m_Lens.OrthographicSize, size => _virtualCamera.m_Lens.OrthographicSize = size, targetSize, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true); // Unscaled 대응
            tasks.Add(_focusZoomTween.ToUniTask());

            if (targetPosition != null && MainCamera != null)
            {
                _targetTm = null;
                _focusMoveTween?.Kill();
                _hasManualFocusPosition = true;

                var cameraPosition = MainCamera.transform.position;
                var focusPosition = targetPosition.Value;
                focusPosition.z = cameraPosition.z;

                _focusMoveTween = MainCamera.transform.DOMove(focusPosition, duration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);
                tasks.Add(_focusMoveTween.ToUniTask());
            }
            
            await UniTask.WhenAll(tasks);

            // await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            endAction?.Invoke();
        }

        void ICameraManager.ClearFocus(Action endAction)
        {
            ClearFocusAsync(endAction).Forget();
        }
        
        private async UniTask ClearFocusAsync(Action endAction)
        { 
            var duration = _zoomInOutDuration;

            // virtualCamera.transform.position = Vector3.zero;

            _focusZoomTween?.Kill();
            _focusZoomTween = DOTween.To(() => _virtualCamera.m_Lens.OrthographicSize,
                orthographicSize => _virtualCamera.m_Lens.OrthographicSize = orthographicSize, _defaultOrthographicSize, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true);

            var tasks = new List<UniTask>
            {
                _focusZoomTween.ToUniTask()
            };

            if (_hasManualFocusPosition && MainCamera != null)
            {
                _focusMoveTween?.Kill();
                var returnPosition = _defaultCameraPosition;
                returnPosition.z = MainCamera.transform.position.z;
                _focusMoveTween = MainCamera.transform.DOMove(returnPosition, duration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);
                tasks.Add(_focusMoveTween.ToUniTask());
            }

            await UniTask.WhenAll(tasks);

            _hasManualFocusPosition = false;

            // await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            endAction?.Invoke();
        }
        #endregion
    }
}
