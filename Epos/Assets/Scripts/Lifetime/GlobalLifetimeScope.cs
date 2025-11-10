using System;
using UnityEngine;

using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

using Creator;
using Creature;
using Entities;
using GameSystem;
using Scene;


namespace Lifetime
{
    public class GlobalLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            Debug.Log("GlobalLifetimeScope Configure");

            builder.Register<ResourceManager>(VContainer.Lifetime.Singleton).AsSelf();
            builder.Register<TimeScaleManager>(VContainer.Lifetime.Singleton).As<ITimeScaleManager>();
            

            builder.RegisterComponentOnNewGameObject<AddressableManager>(VContainer.Lifetime.Singleton, $"[{typeof(AddressableManager).Name}]")
               .UnderTransform(transform)
               .AsSelf();

            builder.RegisterComponentOnNewGameObject<InputManager>(VContainer.Lifetime.Singleton, $"[{typeof(InputManager).Name}]")
                .UnderTransform(transform)
                .As<IInputManager>();

            builder.RegisterComponentOnNewGameObject<EffectManager>(VContainer.Lifetime.Singleton, $"[{typeof(EffectManager).Name}]")
                .UnderTransform(transform)
                .As<IEffectManager>();

            builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
            builder.RegisterComponentInHierarchy<CameraManager>().As<ICameraManager>();
            builder.RegisterComponentInHierarchy<Party>().As<IParty>();
            builder.RegisterComponentInHierarchy<SceneInitializer>().AsSelf();

            builder.RegisterComponentOnNewGameObject<ObjectPooler>(VContainer.Lifetime.Singleton, $"[{typeof(ObjectPooler).Name}]").AsSelf();
            builder.Register(typeof(UICreator<,>), VContainer.Lifetime.Transient).AsSelf();
            builder.Register<UIFactory>(VContainer.Lifetime.Singleton);
        }

        protected override void Awake()
        {
            base.Awake();

            Debug.Log("GlobalLifetimeScope Awake");
            DontDestroyOnLoad(this);

            InitalizeAsync().Forget();
        }

        private async UniTask InitalizeAsync()
        {
            Container?.Resolve<ITimeScaleManager>()?.Set(1f);

            var sceneInitializer = Container?.Resolve<SceneInitializer>();
            sceneInitializer?.CreateChild(this);

            await UniTask.Yield();

            await Container.Resolve<ResourceManager>()
                .InitializeAsync();

            await Container.Resolve<UIManager>()
               .InitializeAsync();

            await sceneInitializer.InitializeAsync();
        }
    }
}

