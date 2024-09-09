using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Creature.Action;
using UnityEngine;

using Spine.Unity;

namespace Creature
{
    public interface ICharacterGeneric
    {
        void Initialize();
        void ChainUpdate();
    }

    public abstract class Character : MonoBehaviour, ICharacterGeneric, IActor
    {
        #region Inspector
        [SerializeField] 
        private int id = 0;
        #endregion

        public SkeletonAnimation SkeletonAnimation { get; private set; } = null;

        public Action.IActController IActCtr { get; private set; } = null;

        // 일단 Awake 사용 
        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            IActCtr?.ChainUpdate();
        }
        
        #region ICharacterGeneric
        public virtual void Initialize()
        {
            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            IActCtr = gameObject.AddOrGetComponent<ActController>();
            IActCtr?.Initialize(this);
        }

        void ICharacterGeneric.ChainUpdate()
        {
            
        }
        #endregion
    }
}

