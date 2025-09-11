using System.Collections;
using UnityEngine.Pool;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;

using Common;
using GameSystem;

namespace Common
{
    public class Param
    {
            
    }
    
    public class Component : MonoBehaviour, IPoolable
    {
        [SerializeField]
        protected Transform rootTm = null;

        [Inject] protected ObjectPooler _objectPooler = null;

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
            Extensions.SetActive(rootTm, true);

            _isActivate = true;
        }
        
        public virtual void Deactivate()
        {
            Extensions.SetActive(rootTm, false);

            _isActivate = false;
        }

        #region IPoolable
        GameObject IPoolable.PrefabKey => gameObject;
        Transform IPoolable.Transform => transform;

        public void Return()
        {
            _objectPooler?.Return(this);
        }
        #endregion
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

        public virtual UniTask ActivateAsync(T param)
        {
            base.Activate();

            _param = param;

            return UniTask.CompletedTask;
        }
    }
}

