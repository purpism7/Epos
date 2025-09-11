using UnityEngine;

using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;

using Lifetime;
using GameSystem;
using Entities;

namespace Scene
{
    public abstract class SceneInitializer : MonoBehaviour
    {
        protected LifetimeScope _lifetimeScope = null;

        public void CreateChild(LifetimeScope parentLifetimeScope)
        {
            _lifetimeScope = parentLifetimeScope?.CreateChild(Configure);
        }

        public virtual async UniTask InitializeAsync()
        {

            await _lifetimeScope.Container.Resolve<ICharacterManager>()
                .InitializeAsync();

            await UniTask.Yield();
        }

        protected virtual void Configure(IContainerBuilder builder)
        {
            Debug.Log("here");
            builder.RegisterEntryPoint<CharacterManager>(VContainer.Lifetime.Scoped).As<ICharacterManager>();
        }
    }
}

