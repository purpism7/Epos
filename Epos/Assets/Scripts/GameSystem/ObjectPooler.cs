using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem
{
    public interface IPoolable
    {
        bool IsActivate { get; }
    }

    public class ObjectPooler
    {
        private List<IPoolable> _iPoolableList = new();

        public void Add(IPoolable iPoolable)
        {
            if (iPoolable == null)
                return;

            _iPoolableList?.Add(iPoolable);
        }

        public void Return()
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

