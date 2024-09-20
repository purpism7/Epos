using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    public class LoadSceneManager : Singleton<LoadSceneManager>
    {
        protected override void Initialize()
        {
            // SceneManager.Loa   
        }

        public async UniTask LoadSceneAsync(string name)
        {
            var currSceneName = SceneManager.GetActiveScene().name;
            
            await SceneManager.LoadSceneAsync("Load");
            await SceneManager.LoadSceneAsync(name);
            await SceneManager.UnloadSceneAsync(currSceneName);
            await SceneManager.UnloadSceneAsync("Load");

            Debug.Log("End Load Scene = " + name);
        }
    }
}

