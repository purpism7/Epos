using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;
using GameSystem;

namespace Common
{
    public class ComponentData
    {
            
    }
    
    public class Component : MonoBehaviour, IPoolable
    {
        [SerializeField]
        protected Transform rootTm = null;

        private bool _isActivate = false;

        public virtual void Initialize()
        {
            
        }

        
        public bool IsActivate 
        {
            get
            {
                if (!rootTm)
                    return false;
                
                return _isActivate;
            }
        }
        
        public virtual void Activate()
        {
            _isActivate = true;

            Extensions.SetActive(rootTm, true);
        }
        
        public virtual void Deactivate()
        {
            _isActivate = false;

            Extensions.SetActive(rootTm, false);
            
            
        }
    }
    
    public abstract class Component<T> : Component where T : ComponentData
    {
        protected T _data = null;

        public virtual void Initialize(T data)
        {
            base.Initialize();
            
            _data = data;
        }
        
        public virtual void Activate(T data)
        {
            base.Activate();
            
            _data = data;
        }
    }
}

