using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Cysharp.Threading.Tasks;
using Spine.Unity;

using Creature.Action;


namespace Creature
{
    public abstract class Character : MonoBehaviour, IActor, ICaster, ICombatant
    {
        #region Inspector

        [SerializeField] private int id = 0;
        [SerializeField] private Transform rootTm = null;

        #endregion

        private IStatGeneric _iStatGeneric = null;

        public int Id
        {
            get { return id; }
        }

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
        public ISkillController ISkillCtr { get; protected set; } = null;
        
        public System.Action<IActor> EventHandler { get; private set; } = null;

        #region ICombatant

        public Type.ETeam ETeam { get; private set; } = Type.ETeam.None;
        public Type.EFormation EFormation { get; private set; } = Type.EFormation.None;

        public int Position
        {
            get { return position; }
        }

        #endregion

        #region Temp Stat

        [Header("Temp Stat")] [SerializeField] [Range(1f, 100f)] [Tooltip("전투 시, 공격 순서 (높을 수록 우선 순위로).")]
        private float actionSpeed = 1f;

        [SerializeField] [Range(1f, 100f)] [Tooltip("이동 속도.")]
        private float moveSpeed = 1f;

        [SerializeField] [Range(1f, 100f)] [Tooltip("공격력.")]
        private float attack = 1f;

        [SerializeField] [Range(0f, 100f)] [Tooltip("공격 시, 공격 할 적과의 거리 (0 일 경우, 제자리에서 공격).")]
        private float attackRange = 1f;

        [SerializeField] 
        [Range(0f, 100f)]
        private float maxHp = 1f;
        
        [SerializeField] 
        [Range(1f, 5f)] private float activePoint = 1f;
        [SerializeField] 
        [Range(1f, 5f)] private float passivePoint = 1f;

        [SerializeField] private int position = 0;

        #endregion

        public bool IsActivate
        {
            get
            {
                if (!rootTm)
                    return false;

                return rootTm.gameObject.activeSelf;
            }
        }

        public abstract string AnimationKey<T>(Act<T> act) where T : Act<T>.BaseData;

        #region ICharacterGeneric

        public virtual void Initialize()
        {
            // EventHandler = null;

            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            _iStatGeneric = new Stat();
            _iStatGeneric?.Initialize(this);

            ISkillCtr = transform.AddOrGetComponent<SkillController>();
            ISkillCtr?.Initialize(this);

            SetOriginStat();
        }

        public virtual void ChainUpdate()
        {
            if (!IsActivate)
                return;

            IActCtr?.ChainUpdate();
        }

        public virtual void ChainFixedUpdate()
        {
            if (!IsActivate)
                return;

            IActCtr?.ChainFixedUpdate();
        }

        public virtual void Activate()
        {
            _iStatGeneric?.Activate();
            IActCtr?.Activate();
            ISkillCtr?.Activate();

            Extensions.SetActive(rootTm, true);
        }

        public virtual void Deactivate()
        {
            _iStatGeneric?.Deactivate();
            IActCtr?.Deactivate();
            ISkillCtr?.Deactivate();

            EventHandler = null;

            Extensions.SetActive(rootTm, false);
        }

        #endregion

        public void EnableNavmeshAgent()
        {
            NavMeshAgent = SkeletonAnimation?.AddOrGetComponent<NavMeshAgent>();
            if (NavMeshAgent != null)
            {
                NavMeshAgent.enabled = true;

                NavMeshAgent.baseOffset = 0.5f;
                NavMeshAgent.speed = 3.5f;
                NavMeshAgent.angularSpeed = 200f;
                NavMeshAgent.acceleration = 100f;
                NavMeshAgent.radius = 0.5f;
                NavMeshAgent.height = 2f;

                NavMeshAgent.updateRotation = false;
                NavMeshAgent.updateUpAxis = false;

                NavMeshAgent.isStopped = false;
                NavMeshAgent.ResetPath();
            }
        }

        public void DisableNavmeshAgent()
        {
            if (NavMeshAgent == null)
                return;

            NavMeshAgent.isStopped = true;
            NavMeshAgent.enabled = false;
        }

        #region IActor
        void IActor.Add(System.Action<IActor> eventHandler)
        {
            EventHandler += eventHandler;
        }
        
        void IActor.Remove(System.Action<IActor> eventHandler)
        {
            EventHandler -= eventHandler;
        }
        
        #endregion

        #region ICombatant

        void ICombatant.SetETeam(Type.ETeam eTeam)
        {
            ETeam = eTeam;
        }

        void ICombatant.SetEFormation(Type.EFormation eFormation)
        {
            EFormation = eFormation;
        }

        #endregion

        #region Temp Stat

        private void SetOriginStat()
        {
            IStat?.SetOrigin(Stat.EType.ActionSpeed, actionSpeed);
            IStat?.SetOrigin(Stat.EType.MoveSpeed, moveSpeed);
            IStat?.SetOrigin(Stat.EType.Attack, attack);
            IStat?.SetOrigin(Stat.EType.AttackRange, attackRange);
            IStat?.SetOrigin(Stat.EType.Hp, maxHp);
            IStat?.SetOrigin(Stat.EType.MaxHp, maxHp);

            IStat?.SetOrigin(Stat.EType.ActivePoint, activePoint);
            IStat?.SetOrigin(Stat.EType.PassivePoint, passivePoint);
        }

        #endregion
    }
}
