using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace GameSystem
{
    public class AddressableManager : MonoBehaviour //Singleton<AddressableManager>
    {
        // private Dictionary<string, Object> _cachedDic = null;
        
        // protected override void Initialize()
        // {
        //     
        // }

        public async UniTask<T> LoadAssetByNameAsync<T>(string addressableName) 
            where T : Object
        {
            var handler = Addressables.LoadAssetAsync<T>(addressableName);

            if (!handler.IsValid())
                return default;

            await handler.ToUniTask();

            var result = handler.Result;
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
        
        // 라벨(Label)이나 주소를 통해 여러 에셋을 한꺼번에 로드하는 함수
        public async UniTask<IList<T>> LoadAssetsAsync<T>(string key) where T : Object
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key, null);
            try 
            {
                // 2. 비동기 대기 (WaitForCompletion 절대 금지!)
                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableManager] '{key}' 로드 중 예외 발생: {e.Message}");
            }

            // 실패 시 안전하게 null이나 빈 리스트 반환
            Debug.LogError($"[AddressableManager] '{key}' 로드 실패 (Status: {handle.Status})");
            return null;
        }
    }
}

