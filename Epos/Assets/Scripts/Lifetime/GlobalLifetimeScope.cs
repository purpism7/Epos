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
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private SceneInitializer sceneInitializer;
        [SerializeField] private Party party;

        // 씬을 다시 로드해도 껍데기(Scope)가 증식하지 않도록 방어
        private static GlobalLifetimeScope _instance;

        protected override void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            base.Awake();

            Debug.Log("GlobalLifetimeScope Awake");
            InitalizeAsync().Forget();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            Debug.Log("GlobalLifetimeScope Configure");

            builder.Register<ResourceManager>(VContainer.Lifetime.Singleton).AsSelf();
            builder.Register<TimeScaleManager>(VContainer.Lifetime.Singleton).As<ITimeScaleManager>();
            

            builder.RegisterComponentOnNewGameObject<AddressableManager>(VContainer.Lifetime.Singleton, $"[{typeof(AddressableManager).Name}]")
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

            // 방법 A: 인스펙터에서 연결한 레퍼런스 등록 (권장: 성능이 좋고 직관적임)
            if (uiManager != null)
                builder.RegisterComponent(uiManager).AsSelf();

            if (cameraManager != null) 
                builder.RegisterComponent(cameraManager).As<ICameraManager>();

            if (sceneInitializer != null) 
                builder.RegisterComponent(sceneInitializer).AsSelf();

            if (party != null)
                builder.RegisterComponent(party).As<IParty>();

            //builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
            //builder.RegisterComponentInHierarchy<CameraManager>().As<ICameraManager>();
            //builder.RegisterComponentInHierarchy<Party>().As<IParty>();
            //builder.RegisterComponentInHierarchy<SceneInitializer>().AsSelf();

            builder.Register(typeof(UICreator<,>), VContainer.Lifetime.Transient).AsSelf();
            builder.Register<UIFactory>(VContainer.Lifetime.Singleton);
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

