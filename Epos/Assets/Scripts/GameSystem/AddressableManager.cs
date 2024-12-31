using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Cysharp.Threading.Tasks;

namespace GameSystem
{
    public class AddressableManager : Singleton<AddressableManager>
    {
        // private Dictionary<string, Object> _cachedDic = null;
        
        protected override void Initialize()
        {
            
        }

        public T LoadAssetByNameAsync<T>(string addressableName) where T : Object
        {
            // if (_cachedDic == null)
            // {
            //     _cachedDic = new();
            //     _cachedDic.Clear();
            // }
            //
            // if (_cachedDic.TryGetValue(name, out Object obj))
            //     return obj as T;
            
            Debug.Log(addressableName);
            var handler = Addressables.LoadAssetAsync<T>(addressableName);
            if (!handler.IsValid())
                return default(T);

            handler.WaitForCompletion();

            var result = handler.Result;
            // _cachedDic?.Add(addressableName, result);
            
            return result;
        }
    }
}

