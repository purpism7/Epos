using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public interface IPoolable
    {
        GameObject PrefabGameObj { get; }

        Transform Transform { get; }
        bool IsActivate { get; }

        void Return(bool setParent);
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

        public void Return(IPoolable iPoolable, bool setParent)
        {
            if (iPoolable == null)
                return;

            if(setParent)
                iPoolable.Transform.SetParent(transform);

            iPoolable.Transform.SetActive(false);
        }

        public T Get<T>(GameObject prefab = null, string key = "") where T : Component
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
                    if (iPoolable.PrefabGameObj != prefab)
                        continue;
                }

                if(!string.IsNullOrEmpty(key))
                {
                    var replacePrefabKey = iPoolable.PrefabGameObj.name.Replace("(Clone)", "");
                    if (replacePrefabKey != key)
                        continue;
                }

                if (iPoolable is T t)
                {
                    //Extensions.SetActive(iPoolable.Transform, true);
                    iPoolable.Transform.gameObject.SetActive(true);
                    return t;
                }
            }

            return null;
        }
    }
}

