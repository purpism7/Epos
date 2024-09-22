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
        protected override void Initialize()
        {
            // SceneManager.Loa   
            DontDestroyOnLoad(this);
        }

        // 임시
        public async UniTask LoadSceneAsync(string name)
        {
            var currSceneName = SceneManager.GetActiveScene().name;
            Fade fade = null;
            
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
            await UniTask.Yield();
            
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
                                    
                        };
                });

            Debug.Log("End Load Scene = " + name);
        }
    }
}

