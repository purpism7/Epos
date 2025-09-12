using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public interface IPoolable
    {
        GameObject PrefabKey { get; }

        Transform Transform { get; }
        bool IsActivate { get; }

        void Return();
    }

    public class ObjectPooler : MonoBehaviour
    {
        private List<IPoolable> _iPoolableList = new();

        public void Add(IPoolable iPoolable)
        {
            if (iPoolable == null)
                return;

            _iPoolableList?.Add(iPoolable);
        }

        public void Return(IPoolable iPoolable)
        {
            if (iPoolable == null)
                return;

            // iPoolable.Transform.SetParent(transform);
            iPoolable.Transform.SetActive(false);
        }

        public T Get<T>(GameObject prefab = null) where T : Component
        {
            if (_iPoolableList.IsNullOrEmpty())
                return null;

            for(int i = 0; i < _iPoolableList.Count; ++i)
            {
                var iPoolable = _iPoolableList[i];
                if (iPoolable == null)
                    continue;

                if (iPoolable.Transform &&
                    iPoolable.Transform.gameObject.activeSelf)
                    continue;

                if(prefab)
                {
                    if (iPoolable.PrefabKey != prefab)
                        continue;
                }

                if (iPoolable is T t)
                {
                    iPoolable.Transform.gameObject.SetActive(true);
                    return t;
                }
            }

            return null;
        }
    }
}

