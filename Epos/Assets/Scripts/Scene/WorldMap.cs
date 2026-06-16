using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

using GameSystem;
using Spine;
using Spine.Unity;
using VContainer;


namespace Scene
{
    public class WorldMap : SceneInitializer
    {
        [SerializeField] private Animator animator = null;
        [SerializeField] private SkeletonAnimation mapSkeletonAnimation = null;
        [SerializeField] private SkeletonAnimation cloudSkeletonAnimation = null;
        [SerializeField] private string mapIdleAnimationName = "Idle_Map";
        [SerializeField] private string cloudCoverAnimationName = "Idle_Cloud";
        [SerializeField] private string cloudFallbackIntroAnimationName = "Start_Cloud";
        [SerializeField] private string cloudIdleAnimationName = "Idle_Cloud";
        [SerializeField] private float cloudIntroStartDelay = 0f;
        [SerializeField] private float cloudIntroToIdleMixDuration = 0.12f;
        [Header("Map Select")]
        [SerializeField] private GameObject mapSelectEffectObject = null;
        [SerializeField] private SkeletonAnimation selectArrowSkeletonAnimation = null;
        [SerializeField] private SkeletonAnimation characterSkeletonAnimation = null;
        [SerializeField] private string mapSelectEffectObjectName = "Eff_WorldMap_Select";
        [SerializeField] private string selectArrowObjectName = "Spine(UI_Select_Arrow_01)";
        [SerializeField] private string characterObjectName = "Spine(WorldMap_Ch_01)";
        [SerializeField] private string selectArrowStartAnimationName = "Start";
        [SerializeField] private string selectArrowIdleAnimationName = "Idle_01";
        [SerializeField] private string characterStartAnimationName = "Start";
        [SerializeField] private string characterIdleAnimationName = "Idle";
        [SerializeField] private float mapSelectEffectTouchRadius = 1.6f;
        [SerializeField] private Vector3 fallbackMapSelectEffectOffset = new Vector3(0.1f, 3.45f, 0f);
        [SerializeField] private Vector3 fallbackSelectArrowOffset = new Vector3(0.2f, 3.1f, 0f);
        [SerializeField] private Vector3 selectArrowAdditionalOffset = new Vector3(0f, 0.35f, 0f);
        [SerializeField] private Vector3 fallbackCharacterOffset = new Vector3(0.2f, 3.35f, 0f);
        [SerializeField] private float characterSceneTransitionDelay = 3f;
        [SerializeField] private string fieldSceneName = "RealTimeField";
        [Header("Camera Focus")]
        [SerializeField] private int firstAreaFocusIndex = 1;
        [SerializeField] private float firstAreaFocusOrthographicSize = 3.2f;

        [Inject] private ICameraManager _iCameraManager = null;

        private readonly List<Area> _mapSelectAreas = new();
        private Area _selectedArea = null;
        private Vector3 _mapSelectEffectOffset = Vector3.zero;
        private Vector3 _selectArrowOffset = Vector3.zero;
        private Vector3 _characterOffset = Vector3.zero;
        private Vector3 _cloudCoverLocalPosition = Vector3.zero;
        private bool _canSelectMap = false;
        private bool _isTransitioningToField = false;

        private void OnEnable()
        {
            ApplyInitialPresentationState();
        }
        
        protected override async UniTask OnInitializeAsync()
        {
            await base.OnInitializeAsync();

            CacheMapSelectReferences();
            PrepareMapSelectState();
            PrepareCloudIntroState();

            await WaitForCloudIntroStartAsync();

            if (cloudIntroStartDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(cloudIntroStartDelay),
                    cancellationToken: this.GetCancellationTokenOnDestroy());

            PlayCloudIntro();
        }

        private void ApplyInitialPresentationState()
        {
            if (!selectArrowSkeletonAnimation)
                selectArrowSkeletonAnimation = FindChildComponent<SkeletonAnimation>(selectArrowObjectName);

            if (!characterSkeletonAnimation)
                characterSkeletonAnimation = FindChildComponent<SkeletonAnimation>(characterObjectName);

            if (selectArrowSkeletonAnimation)
                selectArrowSkeletonAnimation.gameObject.SetActive(false);

            if (characterSkeletonAnimation)
                characterSkeletonAnimation.gameObject.SetActive(false);

            if (cloudSkeletonAnimation)
            {
                cloudSkeletonAnimation.gameObject.SetActive(true);
                _cloudCoverLocalPosition = cloudSkeletonAnimation.transform.localPosition;
                ApplyCloudStartPose();
            }
        }

        private void Update()
        {
            if (!_canSelectMap)
                return;

            if (!Input.GetMouseButtonUp(0))
                return;

            var area = GetClickedMapSelectArea();
            if (!area)
            {
                DeselectMapArea();
                return;
            }

            if (!_selectedArea)
            {
                SelectMapArea(area);
                return;
            }

            if (_selectedArea != area)
            {
                DeselectMapArea();
                return;
            }

            PlayCharacterOnSelectedArea(area);
        }

        private void PrepareCloudIntroState()
        {
            mapSkeletonAnimation?.PlayAnimation(mapIdleAnimationName, true, null, out _);

            if (!cloudSkeletonAnimation)
                return;

            ApplyCloudStartPose();
        }

        private void ApplyCloudStartPose()
        {
            if (!cloudSkeletonAnimation)
                return;

            cloudSkeletonAnimation.gameObject.SetActive(true);
            cloudSkeletonAnimation.transform.localPosition = _cloudCoverLocalPosition;

            if (!cloudSkeletonAnimation.PlayAnimation(cloudFallbackIntroAnimationName, false, null, out _))
            {
                cloudSkeletonAnimation.PlayAnimation(cloudCoverAnimationName, true, null, out _);
                return;
            }

            var trackEntry = cloudSkeletonAnimation.AnimationState?.GetCurrent(0);
            if (trackEntry != null)
            {
                trackEntry.TrackTime = 0f;
                trackEntry.TimeScale = 0f;
            }

            cloudSkeletonAnimation.Update(0f);
        }

        private async UniTask WaitForCloudIntroStartAsync()
        {
            if (!LoadSceneManager.Validate())
                return;

            var loadSceneManager = LoadSceneManager.Instance;
            await UniTask.WaitUntil(
                () => loadSceneManager.IsDestinationSceneReady ||
                      loadSceneManager.IsSceneRevealStarted ||
                      !loadSceneManager.IsLoading,
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        private void PlayCloudIntro()
        {
            if (!cloudSkeletonAnimation)
            {
                EnableMapSelection();
                return;
            }

            cloudSkeletonAnimation.gameObject.SetActive(true);
            cloudSkeletonAnimation.transform.localPosition = _cloudCoverLocalPosition;

            if (PlayCloudIntroToIdle())
                return;

            Debug.LogWarning($"Cloud intro animation '{cloudFallbackIntroAnimationName}' was not found. Cloud idle loop will continue.");
            PlayCloudIdleLoop();
        }

        private bool PlayCloudIntroToIdle()
        {
            var animationState = cloudSkeletonAnimation?.AnimationState;
            if (animationState == null)
                return false;

            var introAnimation = ResolveCloudAnimation(cloudFallbackIntroAnimationName);
            if (introAnimation == null)
                return false;

            animationState.ClearTracks();
            var introEntry = animationState.SetAnimation(0, introAnimation, false);
            if (introEntry == null)
                return false;

            var idleAnimation = ResolveCloudAnimation(cloudIdleAnimationName);
            if (idleAnimation != null)
            {
                var idleEntry = animationState.AddAnimation(0, idleAnimation, true, 0f);
                idleEntry?.SetMixDuration(Mathf.Max(0f, cloudIntroToIdleMixDuration), 0f);
            }
            else
            {
                introEntry.Complete += _ => PlayCloudIdleLoop();
            }

            introEntry.Complete += _ => EnableMapSelection();
            return true;
        }

        private void PlayCloudIdleLoop()
        {
            if (!cloudSkeletonAnimation)
            {
                EnableMapSelection();
                return;
            }

            cloudSkeletonAnimation.gameObject.SetActive(true);
            cloudSkeletonAnimation.transform.localPosition = _cloudCoverLocalPosition;
            var animationState = cloudSkeletonAnimation.AnimationState;
            var idleAnimation = ResolveCloudAnimation(cloudIdleAnimationName);
            if (animationState != null && idleAnimation != null)
                animationState.SetAnimation(0, idleAnimation, true);
            else
                cloudSkeletonAnimation.PlayAnimation(cloudIdleAnimationName, true, null, out _);

            EnableMapSelection();
        }

        private Spine.Animation ResolveCloudAnimation(string animationName)
        {
            if (cloudSkeletonAnimation?.AnimationState?.Data?.SkeletonData == null ||
                string.IsNullOrEmpty(animationName))
            {
                return null;
            }

            var skeletonData = cloudSkeletonAnimation.AnimationState.Data.SkeletonData;
            var animation = skeletonData.FindAnimation(animationName);
            if (animation != null)
                return animation;

            var animations = skeletonData.Animations;
            if (animations == null)
                return null;

            for (int i = 0; i < animations.Count; i++)
            {
                animation = animations.Items[i];
                if (animation != null && animation.Name.Contains(animationName))
                    return animation;
            }

            return null;
        }

        private void CacheMapSelectReferences()
        {
            _mapSelectAreas.Clear();
            _mapSelectAreas.AddRange(GetComponentsInChildren<Area>(true));

            if (!mapSelectEffectObject)
                mapSelectEffectObject = FindChildGameObject(mapSelectEffectObjectName);

            if (!selectArrowSkeletonAnimation)
                selectArrowSkeletonAnimation = FindChildComponent<SkeletonAnimation>(selectArrowObjectName);

            if (!characterSkeletonAnimation)
                characterSkeletonAnimation = FindChildComponent<SkeletonAnimation>(characterObjectName);

            if (cloudSkeletonAnimation)
                _cloudCoverLocalPosition = cloudSkeletonAnimation.transform.localPosition;

            var anchorArea = GetMapSelectArea(1);
            if (!anchorArea && _mapSelectAreas.Count > 0)
                anchorArea = _mapSelectAreas[0];

            _mapSelectEffectOffset = fallbackMapSelectEffectOffset;
            if (anchorArea && mapSelectEffectObject)
                _mapSelectEffectOffset = mapSelectEffectObject.transform.position - anchorArea.transform.position;

            _selectArrowOffset = fallbackSelectArrowOffset;
            if (anchorArea && selectArrowSkeletonAnimation)
                _selectArrowOffset = selectArrowSkeletonAnimation.transform.position - anchorArea.transform.position;

            _characterOffset = fallbackCharacterOffset;
            if (anchorArea && characterSkeletonAnimation)
                _characterOffset = characterSkeletonAnimation.transform.position - anchorArea.transform.position;
        }

        private void PrepareMapSelectState()
        {
            _selectedArea = null;
            _canSelectMap = false;

            SetMapSelectAreasActive(false);

            if (mapSelectEffectObject)
                mapSelectEffectObject.SetActive(true);

            if (selectArrowSkeletonAnimation)
                selectArrowSkeletonAnimation.gameObject.SetActive(false);

            if (characterSkeletonAnimation)
                characterSkeletonAnimation.gameObject.SetActive(false);
        }

        private void EnableMapSelection()
        {
            _canSelectMap = true;
        }

        private void SetMapSelectAreasActive(bool active)
        {
            for (int i = 0; i < _mapSelectAreas.Count; i++)
            {
                var area = _mapSelectAreas[i];
                if (!area)
                    continue;

                area.gameObject.SetActive(active);

                var spriteRenderers = area.GetComponentsInChildren<SpriteRenderer>(true);
                for (int j = 0; j < spriteRenderers.Length; j++)
                {
                    var spriteRenderer = spriteRenderers[j];
                    if (spriteRenderer)
                        spriteRenderer.enabled = active;
                }
            }
        }

        private Area GetClickedMapSelectArea()
        {
            var camera = _iCameraManager?.MainCamera;
            if (camera == null)
                camera = Camera.main;

            if (camera == null)
                return null;

            var worldPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0f;

            var clickedArea = GetMapSelectAreaAtWorldPosition(worldPosition);
            if (clickedArea)
                return clickedArea;

            if (!IsWorldPositionInsideMapSelectEffect(worldPosition))
                return null;

            return _selectedArea ? _selectedArea : GetNearestMapSelectArea(worldPosition, float.MaxValue);
        }

        private bool IsWorldPositionInsideMapSelectEffect(Vector3 worldPosition)
        {
            if (!mapSelectEffectObject || !mapSelectEffectObject.activeInHierarchy)
                return false;

            var colliders = mapSelectEffectObject.GetComponentsInChildren<Collider2D>(true);
            bool hasCollider = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (!collider || !collider.enabled || !collider.gameObject.activeInHierarchy)
                    continue;

                hasCollider = true;
                if (IsWorldPositionInsideCollider(worldPosition, collider))
                    return true;
            }

            if (hasCollider)
                return false;

            var effectSqrDistance = (mapSelectEffectObject.transform.position - worldPosition).sqrMagnitude;
            return effectSqrDistance <= mapSelectEffectTouchRadius * mapSelectEffectTouchRadius;
        }

        private Area GetNearestMapSelectArea(Vector3 worldPosition, float maxSqrDistance)
        {
            Area nearestArea = null;
            var nearestSqrDistance = maxSqrDistance;

            for (int i = 0; i < _mapSelectAreas.Count; i++)
            {
                var area = _mapSelectAreas[i];
                if (!area)
                    continue;

                var sqrDistance = (GetMapSelectTouchCenter(area) - worldPosition).sqrMagnitude;
                if (sqrDistance > nearestSqrDistance)
                    continue;

                nearestSqrDistance = sqrDistance;
                nearestArea = area;
            }

            return nearestArea;
        }

        private Area GetMapSelectAreaAtWorldPosition(Vector3 worldPosition)
        {
            for (int i = 0; i < _mapSelectAreas.Count; i++)
            {
                var area = _mapSelectAreas[i];
                if (!area)
                    continue;

                if (IsWorldPositionInsideArea(worldPosition, area))
                    return area;
            }

            return null;
        }

        private Vector3 GetMapSelectTouchCenter(Area area)
        {
            if (!area)
                return Vector3.zero;

            return area.transform.position + _mapSelectEffectOffset;
        }

        private void SelectMapArea(Area area)
        {
            _selectedArea = area;
            FocusCameraOnAreaIfNeeded(area);

            if (characterSkeletonAnimation)
                characterSkeletonAnimation.gameObject.SetActive(false);

            if (mapSelectEffectObject)
            {
                mapSelectEffectObject.transform.position = area.transform.position + _mapSelectEffectOffset;
                mapSelectEffectObject.SetActive(true);
            }

            if (!selectArrowSkeletonAnimation)
                return;

            selectArrowSkeletonAnimation.transform.position = area.transform.position + _selectArrowOffset + selectArrowAdditionalOffset;
            selectArrowSkeletonAnimation.gameObject.SetActive(true);
            selectArrowSkeletonAnimation.PlayAnimation(selectArrowStartAnimationName, false,
                (trackEntry) =>
                {
                    selectArrowSkeletonAnimation?.PlayAnimation(selectArrowIdleAnimationName, true, null, out _);
                }, out _);
        }

        private void FocusCameraOnAreaIfNeeded(Area area)
        {
            if (!area || area.Index != firstAreaFocusIndex)
                return;

            _iCameraManager?.SetTargetTr(null, Vector3.zero);
            _iCameraManager?.FocusOnTarget(null, firstAreaFocusOrthographicSize, GetMapSelectTouchCenter(area));
        }

        private void DeselectMapArea()
        {
            if (!_selectedArea)
                return;

            var deselectedArea = _selectedArea;
            _selectedArea = null;
            _canSelectMap = false;

            if (selectArrowSkeletonAnimation)
                selectArrowSkeletonAnimation.gameObject.SetActive(false);

            StopMapSelectEffect();

            if (_iCameraManager != null)
                _iCameraManager.ClearFocus(() => ReactivateMapSelectArea(deselectedArea));
            else
                ReactivateMapSelectArea(deselectedArea);
        }

        private void PlayCharacterOnSelectedArea(Area area)
        {
            if (!characterSkeletonAnimation || _isTransitioningToField)
                return;

            _isTransitioningToField = true;
            _canSelectMap = false;

            if (selectArrowSkeletonAnimation)
                selectArrowSkeletonAnimation.gameObject.SetActive(false);

            StopMapSelectEffect();

            characterSkeletonAnimation.transform.position = area.transform.position + _characterOffset;
            characterSkeletonAnimation.gameObject.SetActive(true);
            characterSkeletonAnimation.PlayAnimation(characterStartAnimationName, false,
                (trackEntry) =>
                {
                    characterSkeletonAnimation?.PlayAnimation(characterIdleAnimationName, true, null, out _);
                }, out _);

            TransitionToFieldSceneAsync().Forget();
        }

        private async UniTask TransitionToFieldSceneAsync()
        {
            try
            {
                var delaySeconds = Mathf.Max(0f, characterSceneTransitionDelay);
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds),
                    cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (string.IsNullOrEmpty(fieldSceneName))
                return;

            StopMapSelectEffect();
            await LoadSceneManager.Instance.LoadSceneAsync(fieldSceneName);
        }

        private void StopMapSelectEffect()
        {
            if (!mapSelectEffectObject)
                return;

            var particleSystems = mapSelectEffectObject.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var particleSystem = particleSystems[i];
                if (particleSystem)
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            mapSelectEffectObject.SetActive(false);
        }

        private void ReactivateMapSelectArea(Area area)
        {
            if (_isTransitioningToField)
                return;

            _canSelectMap = true;

            if (!area || !mapSelectEffectObject)
                return;

            mapSelectEffectObject.transform.position = GetMapSelectTouchCenter(area);
            mapSelectEffectObject.SetActive(true);
            PlayMapSelectEffectParticles();
        }

        private void PlayMapSelectEffectParticles()
        {
            if (!mapSelectEffectObject)
                return;

            var particleSystems = mapSelectEffectObject.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var particleSystem = particleSystems[i];
                if (!particleSystem)
                    continue;

                particleSystem.Clear(true);
                particleSystem.Play(true);
            }
        }

        private Area GetMapSelectArea(int index)
        {
            for (int i = 0; i < _mapSelectAreas.Count; i++)
            {
                var area = _mapSelectAreas[i];
                if (area && area.Index == index)
                    return area;
            }

            return null;
        }

        private bool IsWorldPositionInsideArea(Vector3 worldPosition, Area area)
        {
            if (!area)
                return false;

            var colliders = area.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (!collider)
                    continue;

                if (IsWorldPositionInsideCollider(worldPosition, collider))
                    return true;
            }

            return false;
        }

        private bool IsWorldPositionInsideCollider(Vector3 worldPosition, Collider2D collider)
        {
            if (collider is CircleCollider2D circleCollider)
                return IsWorldPositionInsideCircleCollider(worldPosition, circleCollider);

            if (collider is CapsuleCollider2D capsuleCollider)
                return IsWorldPositionInsideCapsuleCollider(worldPosition, capsuleCollider);

            if (collider is BoxCollider2D boxCollider)
                return IsWorldPositionInsideBoxCollider(worldPosition, boxCollider);

            return collider.OverlapPoint(worldPosition);
        }

        private bool IsWorldPositionInsideCircleCollider(Vector3 worldPosition, CircleCollider2D collider)
        {
            var center = collider.transform.TransformPoint(collider.offset);
            var scale = collider.transform.lossyScale;
            var radius = collider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            return (worldPosition - center).sqrMagnitude <= radius * radius;
        }

        private bool IsWorldPositionInsideCapsuleCollider(Vector3 worldPosition, CapsuleCollider2D collider)
        {
            var localPosition = collider.transform.InverseTransformPoint(worldPosition) - (Vector3)collider.offset;
            var halfSize = collider.size * 0.5f;
            float radius = collider.direction == CapsuleDirection2D.Vertical ? halfSize.x : halfSize.y;

            if (collider.direction == CapsuleDirection2D.Vertical)
            {
                var halfBodyHeight = Mathf.Max(0f, halfSize.y - radius);
                if (Mathf.Abs(localPosition.y) <= halfBodyHeight && Mathf.Abs(localPosition.x) <= radius)
                    return true;

                var capCenterY = localPosition.y > 0f ? halfBodyHeight : -halfBodyHeight;
                var capCenter = new Vector2(0f, capCenterY);
                return ((Vector2)localPosition - capCenter).sqrMagnitude <= radius * radius;
            }

            var halfBodyWidth = Mathf.Max(0f, halfSize.x - radius);
            if (Mathf.Abs(localPosition.x) <= halfBodyWidth && Mathf.Abs(localPosition.y) <= radius)
                return true;

            var capCenterX = localPosition.x > 0f ? halfBodyWidth : -halfBodyWidth;
            var horizontalCapCenter = new Vector2(capCenterX, 0f);
            return ((Vector2)localPosition - horizontalCapCenter).sqrMagnitude <= radius * radius;
        }

        private bool IsWorldPositionInsideBoxCollider(Vector3 worldPosition, BoxCollider2D collider)
        {
            var localPosition = collider.transform.InverseTransformPoint(worldPosition) - (Vector3)collider.offset;
            var halfSize = collider.size * 0.5f;
            return Mathf.Abs(localPosition.x) <= halfSize.x && Mathf.Abs(localPosition.y) <= halfSize.y;
        }

        private T FindChildComponent<T>(string childName) where T : Component
        {
            if (string.IsNullOrEmpty(childName))
                return null;

            var transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                var child = transforms[i];
                if (child && child.name == childName)
                    return child.GetComponent<T>();
            }

            return null;
        }

        private GameObject FindChildGameObject(string childName)
        {
            if (string.IsNullOrEmpty(childName))
                return null;

            var transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                var child = transforms[i];
                if (child && child.name == childName)
                    return child.gameObject;
            }

            return null;
        }
    }
}
