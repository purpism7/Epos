using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class Panel : MonoBehaviour
    {
        [SerializeField]
        private Transform rootTm = null;
        
        public bool IsActivate 
        {
            get
            {
                if (!rootTm)
                    return false;
                
                return rootTm.gameObject.activeSelf;
            }
        }
        
        public virtual void Activate()
        {
            Extensions.SetActive(rootTm, true);
        }
        
        public virtual void Deactivate()
        {
            Extensions.SetActive(rootTm, false);
        }
    }
    
    public abstract class Panel<T> : Panel where T : Panel<T>.Base
    {
        public class Base
        {
            
        }

        protected T _data = null;

        public virtual Panel<T> Initialize(T data)
        {
            _data = data;

            return this;
        }

        public virtual void Activate(T data = null)
        {
            base.Activate();

            if (data == null)
                _data = data;
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}


