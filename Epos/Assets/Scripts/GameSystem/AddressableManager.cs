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
        // 로드한 prefab의 핸들을 addressable name 별로 캐시. 같은 name을 여러 번 로드해도
        // Addressables 자체가 ref count를 관리하므로 핸들은 한 번만 보관.
        private readonly Dictionary<string, AsyncOperationHandle> _handlesByName = new();
        // Instantiate된 GameObject가 어느 addressable name에서 왔는지 매핑. ReleaseInstance 시 사용.
        private readonly Dictionary<int, string> _nameByInstanceId = new();

        public async UniTask<T> LoadAssetByNameAsync<T>(string addressableName)
            where T : Object
        {
            // 같은 name 재요청 시 캐시된 핸들 결과 반환 (Addressables 내부 ref count 증가 안 함)
            if (_handlesByName.TryGetValue(addressableName, out var cached) && cached.IsValid())
                return cached.Result as T;

            var handler = Addressables.LoadAssetAsync<T>(addressableName);

            if (!handler.IsValid())
                return default;

            await handler.ToUniTask();

            // 핸들을 보관 — 명시적으로 ReleaseInstance/ReleaseByName/ReleaseAll로 해제할 때까지 유지.
            // 이렇게 안 하고 즉시 Release하면 Instantiate된 인스턴스의 sharedMaterial이 background unload로 null이 됨.
            _handlesByName[addressableName] = handler;

            return handler.Result;
        }

        /// <summary>Instantiate된 GameObject를 어떤 addressable name에서 왔는지 등록.
        /// 추후 ReleaseInstance(instance) 호출 시 정확히 해당 핸들을 정리할 수 있게 함.</summary>
        public void TrackInstance(GameObject instance, string addressableName)
        {
            if (instance == null || string.IsNullOrEmpty(addressableName))
                return;

            _nameByInstanceId[instance.GetInstanceID()] = addressableName;
        }

        /// <summary>인스턴스가 더 이상 필요 없을 때 호출. 같은 name을 사용하는 다른 인스턴스가
        /// 남아있지 않으면 해당 핸들을 Addressables에 release한다.</summary>
        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
                return;

            var id = instance.GetInstanceID();
            if (!_nameByInstanceId.TryGetValue(id, out var name))
                return;

            _nameByInstanceId.Remove(id);

            foreach (var kvp in _nameByInstanceId)
            {
                if (kvp.Value == name)
                    return; // 같은 prefab을 쓰는 인스턴스가 남아있음 → 핸들 유지
            }

            ReleaseByName(name);
        }

        /// <summary>특정 addressable name의 핸들을 강제 해제. 호출 후 그 name으로 만들어진
        /// 인스턴스들의 sharedMaterial이 unload되어 깨질 수 있으므로, 모든 인스턴스가
        /// 정리된 뒤에만 호출할 것.</summary>
        public void ReleaseByName(string addressableName)
        {
            if (!_handlesByName.TryGetValue(addressableName, out var handle))
                return;

            _handlesByName.Remove(addressableName);

            if (handle.IsValid())
                Addressables.Release(handle);
        }

        /// <summary>모든 캐시된 핸들 해제. 씬 전환이나 게임 종료 시 호출.</summary>
        public void ReleaseAll()
        {
            foreach (var handle in _handlesByName.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            _handlesByName.Clear();
            _nameByInstanceId.Clear();
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

