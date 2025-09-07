using UnityEngine;
using UnityEngine.AI;
using System;

using VContainer;
using Spine;
using Spine.Unity;
using Cysharp.Threading.Tasks;

using Common;
using Creator;
using Creature.Action;
using GameSystem;
using UI.Parts;

namespace Creature
{
    public abstract class Character : Common.Component, IActor, Stat.IListener
    {

        #region Inspector

        [SerializeField] private int id = 0;
        [SerializeField] private EClass eClass = EClass.None;
        //[SerializeField] private Transform rootTm = null;

        #endregion

        private MeshRenderer _meshRenderer = null;
        private IStatGeneric _iStatGeneric = null;
        // private int _partyPosition = 0;

        public int Id
        {
            get { return id; }
        }

        public float Height { get; private set; } = 0;

        public SkeletonAnimation SkeletonAnimation { get; private set; } = null;

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
        // public ISkillController ISkillCtr { get; protected set; } = null;
        public Combatant ICombatant { get; protected set; } = null;
        
        #region ICombatant

        public Common.ETeam ETeam { get; private set; } = Common.ETeam.None;
        // public EFormation EFormation { get; private set; } = EFormation.None;

        // public int PartyPosition => _partyPosition;

        #endregion

        [Inject] protected IObjectResolver _iResolver = null;
        // [Inject] private IBattleManager _iBattleManager = null;
        // [Inject] private ResourceManager _resourceManager = null;
        // [Inject] private UIFactory _uiFactory = null;

        #region Temp Stat

        [Header("Temp Stat")] [SerializeField] [Range(1f, 100f)] [Tooltip("전투 시, 공격 순서 (높을 수록 우선 순위로).")]
        private float actionSpeed = 1f;

        [SerializeField] [Range(1f, 100f)] [Tooltip("이동 속도.")]
        private float moveSpeed = 1f;

        [SerializeField] [Range(1f, 100f)] [Tooltip("공격력.")]
        private float attack = 1f;

        // [SerializeField] [Range(0f, 100f)] [Tooltip("공격 시, 공격 할 적과의 거리 (0 일 경우, 제자리에서 공격).")]
        // private float attackRange = 1f;

        [SerializeField] [Range(0f, 100f)] private float maxHp = 1f;

        [SerializeField] [Range(1, 5)] private float activePoint = 1f;
        [SerializeField] [Range(1, 5)] private float passivePoint = 1f;
        
        [SerializeField] [Range(1f, 20f)] private float attackSight = 10f;

        // [SerializeField] private int position = 0;

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
        private void InjectInitialize()
        {
            Debug.Log("Inject Initialize");
        }
        
        #region ICharacterGeneric
        public override void Initialize()
        {
            // EventHandler = null;
            base.Initialize();

            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            _meshRenderer = SkeletonAnimation?.GetComponent<MeshRenderer>();

            _iStatGeneric = new Stat();
            _iStatGeneric?.Initialize(this);

            // IActCtr = transform.AddOrGetComponent<ActController>();
            // _iResolver?.Inject(IActCtr);
            // IActCtr?.Initialize(this);

            // ISkillCtr = transform.AddOrGetComponent<SkillController>();
            // ISkillCtr?.Initialize(this);

            SetOriginStat();
            
            // Renderer renderer = GetComponentInChildren<Renderer>();
            // if (renderer != null)
            // {
            //     Height = renderer.bounds.size.y;
            //     Debug.Log(Height);
            //     //HeadPos = renderer.bounds.max;
            // }

            EnableNavmeshAgent();

            if (NavMeshAgent != null)
                Height = NavMeshAgent.height;
        }

        public virtual void ChainUpdate()
        {
            if (!IsActivate)
                return;

            IActCtr?.ChainUpdate();
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
            // ISkillCtr?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            _iStatGeneric?.Deactivate();
            IActCtr?.Deactivate();
            // ISkillCtr?.Deactivate();      
        }
        #endregion

        protected void InitializeActController(IActor iActor)
        {
            IActCtr = new ActController();
            _iResolver?.Inject(IActCtr);
            IActCtr?.Initialize(iActor);
        }

        // protected void InitializeSkillController(ICaster iCaster)
        // {
        //     ISkillCtr = transform.AddOrGetComponent<SkillController>();
        //     ISkillCtr?.Initialize(iCaster);
        // }

        protected void CreateCombatant()
        {
            ICombatant = new(this);
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
        void IActor.SortingOrder(float order)
        {
            var sortingOrder = Mathf.CeilToInt(-order * 100f);

            if (_meshRenderer == null)
                _meshRenderer = SkeletonAnimation?.GetComponent<MeshRenderer>();

            if (_meshRenderer != null)
                _meshRenderer.sortingOrder = sortingOrder;
        }
        #endregion

        #region ICombatant
        // void ICombatant.SetETeam(ETeam eTeam)
        // {
        //     ETeam = eTeam;
        // }
        
        // void ICombatant.SetPartyPosition(int partyPosition)
        // {
        //     _partyPosition = partyPosition;
        // }

        // void ICombatant.SetPosition(Vector3 pos)
        // {
        //     transform.position = pos;
        // }

        //void ICombatant.CreateHpProgress()
        //{
        //    CreateHpProgress();
        //}

        //private void CreateHpProgress()
        //{
        //    // if (!Transform)
        //    //     return;

        //    // if (_iHpProgress == null)
        //    // {
        //    //     var uiCreator = _uiFactory?.Create<HpProgress, HpProgress.Param>();
        //    //     _iHpProgress = uiCreator?
        //    //         .SetWorldUI(true)?
        //    //         .Create();
        //    // }
            
        //    // var targetPos = Transform.position;
        //    // targetPos.y += Height;

        //    // var param = new HpProgress.Param
        //    // {
        //    //     TargetTm = Transform,
        //    //     Offset = new Vector2(0, Height),
        //    // }.WithCombatant(this);

        //    // _iHpProgress?.Activate(param);
        //}
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

            IStat?.SetOrigin(Stat.EType.ActivePoint, activePoint);
            IStat?.SetOrigin(Stat.EType.PassivePoint, passivePoint);
            
            IStat?.SetOrigin(Stat.EType.AttackSight, attackSight);
        }
        #endregion
        
        #region Stat.IListener
        void Stat.IListener.OnStatChanged(Stat.EType eType, float value)
        {
            if (eType == Stat.EType.Hp)
            {
                //_iHpProgress?.UpdateHpProgress();
                
                if(value <= 0)
                    IActCtr?.Die();
            }
        }
        #endregion
    }
}
