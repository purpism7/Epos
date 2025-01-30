using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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
            Debug.Log(addressableName);
            var handler = Addressables.LoadAssetAsync<T>(addressableName);
            if (!handler.IsValid())
                return default(T);

            handler.WaitForCompletion();

            var result = handler.Result;
            // _cachedDic?.Add(addressableName, result);
            Addressables.Release(handler);
            
            return result;
        }
        
        public async UniTask LoadAssetAsync<T>(string labelKey, System.Action<AsyncOperationHandle<T>> action)
        {
            var locationAsync = await Addressables.LoadResourceLocationsAsync(labelKey);

            foreach (IResourceLocation resourceLocation in locationAsync)
            {
                var assetAync = Addressables.LoadAssetAsync<T>(resourceLocation);

                await UniTask.WaitUntil(() => assetAync.IsDone);
                if (assetAync.Result == null)
                    continue;

                assetAync.Completed += action;
            }
        }
    }
}

