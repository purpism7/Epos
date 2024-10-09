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
        void Activate();
        void Deactivate();
    }

    public abstract class Character : MonoBehaviour, ICharacterGeneric
    {
        #region Inspector
        [SerializeField] 
        private int id = 0;
        #endregion

        public SkeletonAnimation SkeletonAnimation { get; private set; } = null;
        public Transform Transform { get { return transform; } }

        public Action.IActController IActCtr { get; protected set; } = null;

        // 임시.
        public abstract float MoveSpeed { get; }
        public abstract string AnimationKey<T>(Act<T> act) where T : Act<T>.BaseData;

        #region ICharacterGeneric
        public virtual void Initialize()
        {
            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }
        
        public virtual void ChainUpdate()
        {
            IActCtr?.ChainUpdate();
        }

        public virtual void Activate()
        {
            IActCtr?.Activate();
            
            Extension.SetActive(transform, true);
        }

        public virtual void Deactivate()
        {
            IActCtr?.Deactivate();
            
            Extension.SetActive(transform, false);
        }
        #endregion
    }
}

