using UnityEngine;

using Common;
using GameSystem;
using Spine.Unity;
using VContainer;

namespace Item
{
    public class DropItem : Element
    {
        private const string StartAnimationName = "Start";
        private const string IdleAnimationName = "Idle";
        private const float IdleHoldDuration = 2f;

        public class Param : ElementParam
        {
            public int SortingOrder { get; private set; } = 0;
            public Vector3 Position { get; private set; } = Vector3.zero;

            public Param(int sortingOrder, Vector3 position)
            {
                SortingOrder = sortingOrder;
                Position = position;
            }
        }
        
        // [SerializeField] private SpriteRenderer spriteRenderer = null;
        [SerializeField] private SkeletonAnimation skeletonAnimation = null;
        
        [Inject] private UIManager _uiManager = null;
        
        private Param _param = null;
        private MeshRenderer _meshRenderer = null;

        public float StartAnimationDuration { get; private set; } = 0f;
        public float IdleAnimationHoldDuration => IdleHoldDuration;

        public override void Initialize()
        {
            base.Initialize();
            
            if(skeletonAnimation != null)
                _meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
        }

        public void Activate(Param param)
        {
            base.Activate();

            _param = param;

            PlayStartAnimation();

            SetSortingOrder();
            SetDropPosition();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            
        }

        private void PlayStartAnimation()
        {
            StartAnimationDuration = 0f;
            if (skeletonAnimation == null)
                return;

            skeletonAnimation.PlayAnimation(StartAnimationName, false, null, out var duration);
            StartAnimationDuration = duration;
        }

        public void PlayIdleAnimation()
        {
            skeletonAnimation?.PlayAnimation(IdleAnimationName, true, null, out _);
        }

        private void SetSortingOrder()
        {
            if (_param == null)
                return;

            if (_meshRenderer == null)
                return;

            _meshRenderer.sortingOrder = _param.SortingOrder;
        }

        private void SetDropPosition()
        {
            if (_param == null)
                return;
            
            transform.position = _param.Position;
        }
    }
} 
