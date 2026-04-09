using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using VContainer;
using Cysharp.Threading.Tasks;

using GameSystem;

namespace Common
{
    public interface IItemManager
    {
        TElement Get<TElement>(Transform rootTr, out bool initialize) where TElement : Element;
    }
    
    public class ItemManager : IItemManager
    {
        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private ObjectPooler _objectPooler = null;
        
        private readonly Dictionary<System.Type, Element> _cachedPrefabs = new();
        
        public async UniTask InitializeAsync()
        {
            IList<GameObject> prefabs = await _addressableManager.LoadAssetsAsync<GameObject>("Item");
            
            if (prefabs == null)
                return;
            
            if (prefabs.Count <= 0) 
                return;

            for (int i = 0; i < prefabs.Count; ++i)
            {
                var prefab = prefabs[i];
                if (prefab == null)
                    continue;

                var element = prefab.GetComponent<Element>();
                if(element)
                    _cachedPrefabs[element.GetType()] = element;
            }
        }

        public TElement Get<TElement>(Transform rootTr, out bool initialize) where TElement : Element
        {
            initialize = false;
            
            var element = _objectPooler.Get<TElement>();
            if (element == null)
            {
                if (_cachedPrefabs.TryGetValue(typeof(TElement), out var cachedElement))
                {
                    var gameObj = GameObject.Instantiate(cachedElement, rootTr);
                    element = gameObj.GetComponent<TElement>();

                    if (element != null)
                    {
                        element.Initialize();
                        _objectPooler?.Add(element);

                        initialize = true;
                    }
                }
            }
            
            element?.Activate();

            return element;
        }
    }
}

