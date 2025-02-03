using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class Component : MonoBehaviour
    {
        public class BaseData
        {
            
        }
        
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
}

