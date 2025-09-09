using UnityEngine;

using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;

using Lifetime;
using GameSystem;

namespace Scene
{
    public abstract class SceneInitializer : MonoBehaviour
    {
        public virtual async UniTask InitializeAsync(LifetimeScope parentLifetimeScope)
        {
            await UniTask.CompletedTask;
        }

        protected virtual void Configure(IContainerBuilder builder)
        {
            Debug.Log("here");
        }
    }
}

