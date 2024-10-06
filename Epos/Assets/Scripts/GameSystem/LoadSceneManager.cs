using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    public class LoadSceneManager : Singleton<LoadSceneManager>
    {
        private bool _isLoad = false;
        
        protected override void Initialize()
        {
            DontDestroyOnLoad(this);

            _isLoad = false;
        }

        // 임시
        public async UniTask LoadSceneAsync(string name)
        {
            await UniTask.Yield();
            
            if (_isLoad)
                return;
            
            Debug.Log(_isLoad);

            _isLoad = true;
            
            var currSceneName = SceneManager.GetActiveScene().name;
            Fade fade = null;
            Debug.Log(currSceneName);
            AsyncOperationHandle<SceneInstance> sceneInstance =
                Addressables.LoadSceneAsync("Assets/Scenes/Load.unity", LoadSceneMode.Additive);
            sceneInstance.Completed +=
                (handle) =>
                {
                    foreach (var rootGameObj in handle.Result.Scene.GetRootGameObjects())
                    {
                        if(!rootGameObj)
                            continue;
                        
                        fade = rootGameObj.GetComponentInChildren<Fade>();
                        if (fade != null)
                            break;
                    }
                };

            await UniTask.WaitUntil(() => fade != null);
            await UniTask.WaitForEndOfFrame(this);
            
            fade?.Out(
                () =>
                {
                    AsyncOperationHandle<SceneInstance> sceneInstance =
                        Addressables.LoadSceneAsync($"Assets/Scenes/{name}.unity", LoadSceneMode.Additive);
                    sceneInstance.Completed +=
                        (handle) =>
                        {
                            SceneManager.UnloadSceneAsync(currSceneName);
            
                            fade.In(
                                () =>
                                {
                                    SceneManager.UnloadSceneAsync("Load");
                                });
                          
                            _isLoad = false;
                        };
                });

            // _isLoad = false;
            Debug.Log("End Load Scene = " + name);
        }
    }
}

