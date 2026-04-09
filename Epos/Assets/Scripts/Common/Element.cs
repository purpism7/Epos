using GameSystem;
using UnityEngine;

namespace Common
{
    public class ElementParam
    {
        
    }
    
    public class Element : MonoBehaviour, IPoolable
    {
        public Transform Transform => transform;
        public GameObject PrefabGameObj => gameObject;

        // string IPoolable.Key => GetType().Name;
        //

        public virtual void Initialize()
        {
            
        }

        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public bool IsActivate
        {
            get
            {
                if (!gameObject)
                    return false;
                
                return gameObject.activeSelf;
            }
        }

        void IPoolable.Return(bool setParent)
        {
            
        }
    }
}