using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using VContainer;
using Cysharp.Threading.Tasks;

using GameSystem;

namespace Common
{
    public class ItemManager
    {
        [Inject] private AddressableManager _addressableManager = null;
        
        public async UniTask InitializeAsync()
        {
            IList<GameObject> prefabs = await _addressableManager.LoadAssetsAsync<GameObject>("Item");
            
            if (prefabs == null)
                return;
            
            
            
            if (prefabs.Count <= 0) 
                return;
            
            Debug.Log($"Loaded {prefabs.Count} prefabs");
        }
    }
}

