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
        private string _sceneName = string.Empty;
        private string _loadSceneName = string.Empty;
        private Fade _fade = null;
        
        private bool _isLoad = false;
        
        protected override void Initialize()
        {
            DontDestroyOnLoad(this);

            Reset();
        }

        public async UniTask LoadSceneAsync(string loadSceneName)
        {
            await UniTask.Yield();
            
            if (_isLoad)
                return;
            
            _isLoad = true;
            _loadSceneName = loadSceneName;
            _sceneName = SceneManager.GetActiveScene().name;
            
            AsyncOperationHandle<SceneInstance> sceneInstance =
                Addressables.LoadSceneAsync("Assets/Scenes/Load.unity", LoadSceneMode.Additive);
            sceneInstance.Completed += LoadFade;
        }

        private void LoadFade(AsyncOperationHandle<SceneInstance> handle)
        {
            foreach (var rootGameObj in handle.Result.Scene.GetRootGameObjects())
            {
                if(!rootGameObj)
                    continue;
                        
                var fade = rootGameObj.GetComponentInChildren<Fade>();
                if (fade == null)
                    continue;
                
                FadeOutAsync(fade).Forget();

                break;
            }
        }

        private async UniTask FadeOutAsync(Fade fade)
        {
            _fade = fade;
            
            _fade?.Out(
                () =>
                {
                    AsyncOperationHandle<SceneInstance> sceneInstance =
                        Addressables.LoadSceneAsync($"Assets/Scenes/{_loadSceneName}.unity", LoadSceneMode.Additive);
                    sceneInstance.Completed += UnLoadScene;
                });
        }

        private void UnLoadScene(AsyncOperationHandle<SceneInstance> handle)
        {
            SceneManager.UnloadSceneAsync(_sceneName);

            FadeInAsync().Forget();
        }
        
        private async UniTask FadeInAsync()
        {
            await UniTask.Yield();
            
            _fade?.In(
                () =>
                {
                    SceneManager.UnloadSceneAsync("Load");

                    Reset();
                });
        }

        private void Reset()
        {
            _isLoad = false;
            _sceneName = string.Empty;
            _loadSceneName = string.Empty;
        }
    }
}

