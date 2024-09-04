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
    }

    public abstract class Character : MonoBehaviour, ICharacterGeneric, IActor
    {
        #region Inspector
        [SerializeField] 
        private int id = 0;
        #endregion

        private SkeletonAnimation _skeletonAnimation = null;

        public Action.IActController IActCtr { get; private set; } = null;

        // 일단 Awake 사용 
        private void Awake()
        {
            Initialize();
        }

        #region ICharacterGeneric
        public virtual void Initialize()
        {
            _skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            IActCtr = gameObject.AddOrGetComponent<ActController>();
            IActCtr?.Initialize(this);
        }
        #endregion
    }
}

