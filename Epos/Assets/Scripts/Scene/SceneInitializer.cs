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
        [Inject]
        protected IBattleManager _iBattleManager = null;

        public virtual async UniTask InitializeAsync(LifetimeScope parentLifetimeScope)
        {
            //parentLifetimeScope.CreateChild(Configure);
            await UniTask.CompletedTask;
        }

        protected virtual void Configure(IContainerBuilder builder)
        {

        }
    }
}

