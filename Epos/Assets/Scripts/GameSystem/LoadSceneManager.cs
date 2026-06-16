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

        public bool IsLoading => _isLoad;
        public bool IsDestinationSceneReady { get; private set; } = false;
        public bool IsSceneRevealStarted { get; private set; } = false;
        
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
            IsDestinationSceneReady = false;
            IsSceneRevealStarted = false;
            _loadSceneName = loadSceneName;
            _sceneName = SceneManager.GetActiveScene().name;
            
            AsyncOperationHandle<SceneInstance> sceneInstance =
                Addressables.LoadSceneAsync("Assets/Scenes/Load.unity", LoadSceneMode.Additive);
            sceneInstance.Completed += LoadFade;
        }

        private void LoadFade(AsyncOperationHandle<SceneInstance> handle)
        {
            var loadScene = handle.Result.Scene;
            ConfigureLoadScene(loadScene);

            foreach (var rootGameObj in loadScene.GetRootGameObjects())
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

        private static void ConfigureLoadScene(UnityEngine.SceneManagement.Scene loadScene)
        {
            foreach (var rootGameObject in loadScene.GetRootGameObjects())
            {
                if (!rootGameObject)
                    continue;

                foreach (var camera in rootGameObject.GetComponentsInChildren<Camera>(true))
                    camera.enabled = false;

                foreach (var canvas in rootGameObject.GetComponentsInChildren<Canvas>(true))
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.worldCamera = null;
                    canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 100);
                }
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
            CompleteDestinationLoadAsync(handle).Forget();
        }

        private async UniTaskVoid CompleteDestinationLoadAsync(AsyncOperationHandle<SceneInstance> handle)
        {
            var destinationScene = handle.Result.Scene;
            if (destinationScene.IsValid())
            {
                SceneManager.SetActiveScene(destinationScene);
                IsDestinationSceneReady = true;
            }

            var unloadOperation = SceneManager.UnloadSceneAsync(_sceneName);
            if (unloadOperation != null)
                await UniTask.WaitUntil(() => unloadOperation.isDone);

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            IsSceneRevealStarted = true;
            FadeInAsync().Forget();
        }
        
        private async UniTask FadeInAsync()
        {
            await UniTask.Yield();
            
            _fade?.In(
                () =>
                {
                    CompleteLoadSceneTransitionAsync().Forget();
                });
        }

        private async UniTask CompleteLoadSceneTransitionAsync()
        {
            var unloadOperation = SceneManager.UnloadSceneAsync("Load");
            if (unloadOperation != null)
                await UniTask.WaitUntil(() => unloadOperation.isDone);

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            Reset();
        }

        private void Reset()
        {
            _isLoad = false;
            IsDestinationSceneReady = false;
            IsSceneRevealStarted = false;
            _sceneName = string.Empty;
            _loadSceneName = string.Empty;
        }
    }
}
