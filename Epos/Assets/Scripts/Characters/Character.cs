using UnityEngine;
using System;

using Spine.Unity;

using Actor;
using Characters.Actions;

namespace Characters
{
    public abstract class Character : MonoBehaviour, IActor
    {
        public Transform Transform => transform;
        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;
        public IActionController ActionController => actionController;
        
        protected SkeletonAnimation skeletonAnimation = null;
        protected ActionController actionController = null;
        
        private MeshRenderer _meshRenderer = null;
        
        // 임시
        void Awake()
        {
            Initialize();
        }
        
        public virtual void Initialize()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            actionController = new ActionController(this);
        }

        public virtual void Activate()
        {
            actionController?.Activate();   
        }

        public virtual void Deactivate()
        {
            actionController?.Deactivate();
        }

        public virtual void ChainUpdate()
        {
            actionController?.ChainUpdate();
        }
        
        public void SetSortingOrder(float order)
        {
            var sortingOrder = Mathf.CeilToInt(-order * 100f);

            if (_meshRenderer == null)
            {
                if (SkeletonAnimation == null)
                {
                    // Debug.LogWarning($"Character.SortingOrder: SkeletonAnimation is null. CharacterId: {Id}");
                    return;
                }
                
                _meshRenderer = SkeletonAnimation.GetComponent<MeshRenderer>();
                
                if (_meshRenderer == null)
                {
                    // Debug.LogWarning($"Character.SortingOrder: MeshRenderer is null. CharacterId: {Id}");
                    return;
                }
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

