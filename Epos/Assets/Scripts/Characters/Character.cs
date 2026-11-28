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
        public Transform Transform => _cachedTransform;
        
        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;
        public IActionController ActionController => actionController;
        public CharacterAnimator CharacterAnimator => _characterAnimator;
        public BattleStatsSettings BattleStats => battleStats;
        
        protected SkeletonAnimation skeletonAnimation = null;
        protected ActionController actionController = null;
        
        [Tooltip("스파인 원본 그림이 뒤집지 않은 상태(ScaleX = +1)에서 바라보는 좌우 방향. 좌우 반전은 이 값으로 정하므로 SkeletonRenderer 의 Initial Flip X 는 끈 채로 둔다.")]
        [SerializeField] private HorizontalFacing artFacing = HorizontalFacing.Right;

        [Tooltip("전투 능력치 ⚑(임시값). 판을 시작할 때(플레이 시작 · R 다시 하기) 읽어 전투 시뮬에 넘긴다(판 도중에 바꾸면 다음 판부터). 공격 피해 0 = 공격하지 않는 허수아비, 이동 속도 0 = 제자리.")]
        [SerializeField] private BattleStatsSettings battleStats = new BattleStatsSettings();

        private CharacterAnimator _characterAnimator = null;
        private MeshRenderer _meshRenderer = null;
        
        public override void Initialize()
        {
            base.Initialize();
            
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            _characterAnimator = new CharacterAnimator(skeletonAnimation, artFacing);

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

