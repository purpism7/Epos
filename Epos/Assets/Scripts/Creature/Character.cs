using UnityEngine;
using UnityEngine.AI;

using Spine.Unity;
using VContainer;

using Creature.Action;
using Datas.ScriptableObjects;
using GameSystem.Event;
using Common;

namespace Creature
{
    public abstract class Character : Common.Component, Stat.IListener
    {
        #region Inspector

        [SerializeField] private int id = 0;
        [SerializeField] private EClass eClass = EClass.None;
        //[SerializeField] private Transform rootTm = null;

        #endregion

        [Inject] private IActController _iActCtr = null;
        [Inject] private ICreatureEffectController _iEffectCtr = null;
        // [Inject] private IBattleManager _iBattleManager = null;
        // [Inject] private ResourceManager _resourceManager = null;
        // [Inject] private UIFactory _uiFactory = null;

        protected IStatGeneric _iStatGeneric = null;

        private MeshRenderer _meshRenderer = null;
       
        public int Id
        {
            get { return id; }
        }

        public float Height { get; private set; } = 0;

        public SkeletonAnimation SkeletonAnimation { get; private set; } = null;
        public Collider2D Collider { get; private set; } = null;

        public Transform Transform
        {
            get { return SkeletonAnimation?.transform; }
        }

        /// <summary>루트를 옮기고 스켈레톤은 로컬 0으로 맞춰, 그림자 등 형제 오브젝트가 같이 움직이게 함.</summary>
        public void SetWorldPosition(Vector3 position)
        {
            transform.position = position;
            if (SkeletonAnimation != null)
                SkeletonAnimation.transform.localPosition = Vector3.zero;
        }

        public NavMeshAgent NavMeshAgent { get; private set; } = null;

        public IStat IStat
        {
            get { return _iStatGeneric?.Stat; }
        }

        public Action.IActController IActCtr => _iActCtr;
        public Action.ICreatureEffectController IEffectCtr => _iEffectCtr;
        public Skill[] Skills => skills;

        #region Temp Stat

        [Header("Temp Stat")] [SerializeField] [UnityEngine.Range(1f, 100f)] [Tooltip("전투 시, 공격 순서 (높을 수록 우선 순위로).")]
        private float actionSpeed = 1f;

        [SerializeField] [UnityEngine.Range(1f, 100f)] [Tooltip("이동 속도.")]
        private float moveSpeed = 1f;

        [SerializeField] [UnityEngine.Range(1f, 100f)] [Tooltip("공격력.")]
        private float attack = 1f;

        // [SerializeField] [Range(0f, 100f)] [Tooltip("공격 시, 공격 할 적과의 거리 (0 일 경우, 제자리에서 공격).")]
        // private float attackRange = 1f;

        [SerializeField] [UnityEngine.Range(0f, 500f)] private float maxHp = 1f;
        [SerializeField] [UnityEngine.Range(0f, 100f)] private float maxMp = 1f;

        [SerializeField] [UnityEngine.Range(1, 5)] private float activePoint = 1f;
        [SerializeField] [UnityEngine.Range(1, 5)] private float passivePoint = 1f;
        
        [SerializeField] [UnityEngine.Range(1f, 20f)] private float attackSight = 10f;

        #endregion

        #region Temp Skill
        [SerializeField] private Skill[] skills = null;
        #endregion


        public bool IsAlive { get { return IStat != null ? IStat.Get(Stat.EType.Hp) > 0 : false; } }
        public abstract string AnimationKey<T>(Act<T> act) where T : ActParam;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (IStat == null || !Transform)
                return;

            float attackSight = IStat.Get(Stat.EType.AttackSight);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Transform.position, attackSight);
        }
#endif

        #region ICharacterGeneric
        public override void Initialize()
        {
            // EventHandler = null;
            base.Initialize();

            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            _meshRenderer = SkeletonAnimation?.GetComponent<MeshRenderer>();

            Collider = GetComponentInChildren<Collider2D>();

            _iStatGeneric = new Stat();
            _iStatGeneric?.Initialize(this);

            SetOriginStat();
            
            EnableNavmeshAgent();

            if (NavMeshAgent != null)
                Height = NavMeshAgent.height;
        }

        public virtual void ChainUpdate()
        {
            if (!IsActivate)
                return;

            IActCtr?.ChainUpdate();
            IEffectCtr?.ChainUpdate();
        }

        public virtual void ChainLateUpdate()
        {
            if (!IsActivate)
                return;

            // SyncRootToSkeleton();
            EnsureVisible();
        }

        /// <summary>스킬 전후 껐다 켜는 동작 등으로 숨겨졌을 수 있으므로, 매 프레임 캐릭터가 보이도록 보장.</summary>
        private void EnsureVisible()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (rootTm != null && !rootTm.gameObject.activeSelf)
                rootTm.gameObject.SetActive(true);
        }

        /// <summary>스켈레톤이 이동했을 때(이동/대시 등) 루트를 스켈레톤 위치에 맞춰 그림자 등 형제 오브젝트가 같이 따라가도록 함.</summary>
        /// <remarks>스킬(Casting) 중에는 동기화 생략 → 스켈레톤이 튀어도 루트 유지. 스킬 후 거리 멀면 스켈레톤을 루트로 끌어와 기사가 안 보이는 현상 방지.</remarks>
        private void SyncRootToSkeleton()
        {
            if (SkeletonAnimation == null || SkeletonAnimation.transform == transform)
                return;

            if (IActCtr?.GetCurrentAct() is Casting)
            {
                SkeletonAnimation.transform.position = transform.position;
                SkeletonAnimation.transform.localPosition = Vector3.zero;
                return;
            }

            float sqrDist = (transform.position - SkeletonAnimation.transform.position).sqrMagnitude;
            const float maxSyncSqrDist = 25f;

            if (sqrDist > maxSyncSqrDist)
            {
                SkeletonAnimation.transform.position = transform.position;
                SkeletonAnimation.transform.localPosition = Vector3.zero;
                return;
            }

            transform.position = SkeletonAnimation.transform.position;
            SkeletonAnimation.transform.localPosition = Vector3.zero;
        }

        public virtual void ChainFixedUpdate()
        {
            if (!IsActivate)
                return;

            IActCtr?.ChainFixedUpdate();
        }

        public override void Activate()
        {
            base.Activate();

            _iStatGeneric?.Activate();
            IActCtr?.Activate();
            IEffectCtr?.Activate();
            // ISkillCtr?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            _iStatGeneric?.Deactivate();
            IActCtr?.Deactivate();
            IEffectCtr?.Deactivate();
            // ISkillCtr?.Deactivate();      
        }
        #endregion

        protected void InitializeActController(IActor iActor)
        {
            IActCtr?.Initialize(iActor);
        }

        protected void InitializeEffectController(IActor iActor)
        {
            IEffectCtr?.Initialize(iActor);
        }

        private void EnableNavmeshAgent()
        {
            NavMeshAgent = SkeletonAnimation?.AddOrGetComponent<NavMeshAgent>();
            if (NavMeshAgent != null)
            {
                NavMeshAgent.enabled = true;

                //NavMeshAgent.baseOffset = 0.5f;
                //NavMeshAgent.speed = 3.5f;
                NavMeshAgent.angularSpeed = 100f;
                NavMeshAgent.acceleration = 100f;
                NavMeshAgent.stoppingDistance = 0.1f;
                //NavMeshAgent.radius = 0.5f;
                //NavMeshAgent.height = 2f;
                NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

                NavMeshAgent.transform.localRotation = Quaternion.identity;
                NavMeshAgent.updateRotation = false;
                NavMeshAgent.updateUpAxis = false;

                

                //NavMeshAgent.isStopped = false;
                //NavMeshAgent.ResetPath();
            }
        }

        #region IActor
        public void SortingOrder(float order)
        {
            var sortingOrder = Mathf.CeilToInt(-order * 100f);

            if (_meshRenderer == null)
            {
                if (SkeletonAnimation == null)
                {
                    Debug.LogWarning($"Character.SortingOrder: SkeletonAnimation is null. CharacterId: {Id}");
                    return;
                }
                
                _meshRenderer = SkeletonAnimation.GetComponent<MeshRenderer>();
                
                if (_meshRenderer == null)
                {
                    Debug.LogWarning($"Character.SortingOrder: MeshRenderer is null. CharacterId: {Id}");
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
        #endregion

        #region Temp Stat

        private void SetOriginStat()
        {
            IStat?.SetOrigin(Stat.EType.ActionSpeed, actionSpeed);
            IStat?.SetOrigin(Stat.EType.MoveSpeed, moveSpeed);
            IStat?.SetOrigin(Stat.EType.Attack, attack);
            // IStat?.SetOrigin(Stat.EType.AttackRange, attackRange);
            IStat?.SetOrigin(Stat.EType.Hp, maxHp);
            IStat?.SetOrigin(Stat.EType.MaxHp, maxHp);

            IStat?.SetOrigin(Stat.EType.Mp, 0);
            IStat?.SetOrigin(Stat.EType.MaxMp, maxMp);

            IStat?.SetOrigin(Stat.EType.ActivePoint, activePoint);
            IStat?.SetOrigin(Stat.EType.PassivePoint, passivePoint);
            
            IStat?.SetOrigin(Stat.EType.AttackSight, attackSight);
        }
        #endregion
        
        #region Stat.IListener
        void Stat.IListener.OnStatChanged(Stat.EType eType, float value)
        {
            switch(eType)
            {
                case Stat.EType.Hp:
                    {
                        if (value <= 0)
                            IActCtr?.Die();

                        break;
                    }

                case Stat.EType.Mp:
                    {
                        GameSystem.Event.EventHandler.Notify(new StatChangedEventData(Id, IStat));
                        break;
                    }
            }
        }
        #endregion
    }
}
