using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem
{
    public interface IPoolable
    {
        bool IsActivate { get; }
    }

    public class ObjectPooler : MonoBehaviour
    {
        private List<IPoolable> _iPoolableList = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public T Get<T>() where T : Component
        {
            if (_iPoolableList.IsNullOrEmpty())
                return null;

            for(int i = 0; i < _iPoolableList.Count; ++i)
            {
                var iPoolable = _iPoolableList[i];
                if (iPoolable == null)
                    continue;

                if (iPoolable.IsActivate)
                    continue;

                if (iPoolable is T t)
                    return t;
            }

            return null;
        }
    }
}

