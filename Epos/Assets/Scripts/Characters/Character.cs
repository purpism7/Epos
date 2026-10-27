using UnityEngine;
using System;

using Spine.Unity;

using Actor;
using Characters.Actions;
using Commons;

namespace Characters
{
    public abstract class Character : MonoEntity, IActor
    {
        public Transform Transform => CachedTransform;
        
        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;
        public IActionController ActionController => actionController;
        
        protected SkeletonAnimation skeletonAnimation = null;
        protected ActionController actionController = null;
        
        private MeshRenderer _meshRenderer = null;
        
        public override void Initialize()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            actionController = new ActionController(this);
        }

        public override void Activate()
        {
            actionController?.Activate();

            base.Activate();
        }

        public override void Deactivate()
        {
            actionController?.Deactivate();

            base.Deactivate();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
            
            actionController?.ChainUpdate();
        }
        
        public void SetSortingOrder(float order)
        {
            var sortingOrder = Mathf.CeilToInt(-order * 100f);

            if (_meshRenderer == null)
            {
                if (SkeletonAnimation == null)
                    return;
                
                _meshRenderer = SkeletonAnimation.GetComponent<MeshRenderer>();

                if (_meshRenderer == null)
                    return;
            }

            // MeshRenderer가 비활성화되어 있으면 활성화
            if (!_meshRenderer.enabled)
                _meshRenderer.enabled = true;
            
            // MeshRenderer의 gameObject가 비활성화되어 있으면 활성화
            if (!_meshRenderer.gameObject.activeInHierarchy)
                _meshRenderer.gameObject.SetActive(true);
            
            _meshRenderer.sortingOrder = sortingOrder;
        }
    }
}

