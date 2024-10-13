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

        private IStatGeneric _iStatGeneric = null;
        
        public SkeletonAnimation SkeletonAnimation { get; private set; } = null;
        public Transform Transform { get { return transform; } }

        public IStat IStat { get { return _iStatGeneric?.Stat; } }
        public Action.IActController IActCtr { get; protected set; } = null;
        
        // Stat 으로 변경 예정.
        public abstract float MoveSpeed { get; }
        
        public abstract string AnimationKey<T>(Act<T> act) where T : Act<T>.BaseData;

        #region ICharacterGeneric
        public virtual void Initialize()
        {
            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            _iStatGeneric = new Stat();
            _iStatGeneric?.Initialize(id);
        }
        
        public virtual void ChainUpdate()
        {
            IActCtr?.ChainUpdate();
        }

        public virtual void Activate()
        {
            _iStatGeneric?.Activate();
            IActCtr?.Activate();
            
            Extension.SetActive(transform, true);
        }

        public virtual void Deactivate()
        {
            _iStatGeneric?.Deactivate();
            IActCtr?.Deactivate();
            
            Extension.SetActive(transform, false);
        }
        #endregion
    }
}

