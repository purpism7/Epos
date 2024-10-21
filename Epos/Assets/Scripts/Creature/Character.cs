using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine.Unity;

using Common;
using Creature.Action;


namespace Creature
{
    public abstract class Character : MonoBehaviour, IActor, ICaster
    {
        #region Inspector
        [SerializeField] 
        private int id = 0;
        [SerializeField] 
        private Transform rootTm = null;
        #endregion

        private IStatGeneric _iStatGeneric = null;
        
        public int Id { get { return id; } }
        public SkeletonAnimation SkeletonAnimation { get; private set; } = null;
        public Transform Transform { get { return transform; } }

        public IStat IStat { get { return _iStatGeneric?.Stat; } }
        public Action.IActController IActCtr { get; protected set; } = null;
        public bool IsActivate { get; private set; } = false;
        
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
            if (!IsActivate)
                return;
            
            IActCtr?.ChainUpdate();
        }

        public virtual void Activate()
        {
            _iStatGeneric?.Activate();
            IActCtr?.Activate();
            
            IsActivate = true;
            
            Extension.SetActive(rootTm, true);
        }

        public virtual void Deactivate()
        {
            _iStatGeneric?.Deactivate();
            IActCtr?.Deactivate();
            
            IsActivate = false;
            
            Extension.SetActive(rootTm, false);
        }
        #endregion
    }
}

