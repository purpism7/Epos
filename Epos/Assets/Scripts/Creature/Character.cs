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

        [Inject] protected IObjectResolver _iResolver = null;
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

        public NavMeshAgent NavMeshAgent { get; private set; } = null;

        public IStat IStat
        {
            get { return _iStatGeneric?.Stat; }
        }

        public Action.IActController IActCtr { get; protected set; } = null;
        public Action.ICreatureEffectController IEffectCtr { get; protected set; } = null;
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

            if (IStat == null)
                return;

            if (!Transform)
                return;

            float attackSight = IStat.Get(Stat.EType.AttackSight);
            // Debug.Log(attackSight);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Transform.position, attackSight);
        }
#endif

        [Inject]
        protected virtual void InitializeInject(IObjectResolver iResolver)
        {
            Debug.Log("Inject Initialize");
            _iResolver = iResolver;

            using var scope = iResolver?.CreateScope(
                builder =>
                {
                    builder.Register<ActController>(VContainer.Lifetime.Singleton).As<IActController>();
                    builder.Register<CreatureEffectController>(VContainer.Lifetime.Scoped).As<ICreatureEffectController>();
                });

            IActCtr = scope?.Resolve<IActController>();
            IEffectCtr = scope?.Resolve<ICreatureEffectController>();
        }
        
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
            
            //_iHpProgress?.ChainLateUpdate();
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
                NavMeshAgent.stoppingDistance = 0.5f;
                //NavMeshAgent.radius = 0.5f;
                //NavMeshAgent.height = 2f;

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
                _meshRenderer = SkeletonAnimation?.GetComponent<MeshRenderer>();

            if (_meshRenderer != null)
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
