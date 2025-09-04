using Common;
using Cysharp.Threading.Tasks;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Common
{
    public class Param
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
    
    public abstract class Component<T> : Component where T : Param
    {
        protected T _param = null;

        public virtual UniTask InitializeAsync(T param)
        {
            base.Initialize();

            _param = param;

            return UniTask.CompletedTask;
        }
        
        public virtual void Activate(T param)
        {
            base.Activate();
            
            _param = param;
        }
    }
}

