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
        [SerializeField] private UIManager uiManager;
        // [SerializeField] private CameraManager cameraManager;
        // [SerializeField] private SceneInitializer sceneInitializer;
        [SerializeField] private Party party;

        // private static GlobalLifetimeScope _instance;

        // public bool IsInitialized { get; private set; } = false;

        protected override void Awake()
        {
            // if (_instance != null && _instance != this)
            // {
            //     Destroy(this.gameObject);
            //     return;
            // }
            
            // _instance = this;
            DontDestroyOnLoad(this.gameObject);

            base.Awake();

            // IsInitialized = false;
            // InitializeAsync().Forget();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            Debug.Log("GlobalLifetimeScope Configure");
            builder.Register<ResourceManager>(VContainer.Lifetime.Singleton).AsSelf();
            builder.Register<TimeScaleManager>(VContainer.Lifetime.Singleton).As<ITimeScaleManager>();
            
            builder.RegisterComponentOnNewGameObject<AddressableManager>(VContainer.Lifetime.Singleton, $"[{nameof(AddressableManager)}]")
               .UnderTransform(transform)
               .AsSelf();

            builder.RegisterComponentOnNewGameObject<InputManager>(VContainer.Lifetime.Singleton, $"[{nameof(InputManager)}]")
                .UnderTransform(transform)
                .As<IInputManager>();

            builder.RegisterComponentOnNewGameObject<EffectManager>(VContainer.Lifetime.Singleton, $"[{nameof(EffectManager)}]")
                .UnderTransform(transform)
                .As<IEffectManager>();

            builder.RegisterComponentOnNewGameObject<ObjectPooler>(VContainer.Lifetime.Singleton, $"[{nameof(ObjectPooler)}]")
                .UnderTransform(transform)
                .AsSelf();

            if (uiManager != null)
                builder.RegisterComponent(uiManager).AsSelf();

            // var cameraManager = Container?.Resolve<CameraManager>();
            // if (cameraManager != null) 
            builder.RegisterComponentInHierarchy<CameraManager>()
                .As<ICameraManager>();

            // if (sceneInitializer != null) 
            //     builder.RegisterComponent(sceneInitializer).AsSelf();

            if (party != null)
                builder.RegisterComponent(party).As<IParty>();

            builder.Register(typeof(UICreator<,>), VContainer.Lifetime.Transient).AsSelf();
            builder.Register<UIFactory>(VContainer.Lifetime.Singleton);
        }

        public async UniTask InitializeAsync()
        {
            Container?.Resolve<ITimeScaleManager>()?.Set(1f);

            // var sceneInitializer = Container?.Resolve<SceneInitializer>();
            // sceneInitializer?.CreateChild(this);

            // await UniTask.Yield();

            var resourceManager = Container?.Resolve<ResourceManager>();
            if(resourceManager != null)
                await resourceManager.InitializeAsync();
            // await Container.Resolve<ResourceManager>()
            //     .InitializeAsync();

            await Container.Resolve<UIManager>()
               .InitializeAsync();

            // IsInitialized = true;
            // await sceneInitializer.InitializeAsync();
        }
    }
}

