using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using GameSystem;
using Scene;
using Creature;
using Entities;

namespace Lifetime
{
    public class GlobalLifetimeScope : LifetimeScope
    {
        //[SerializeField] private GameObject uiManagerPrefab = null;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            Debug.Log("GlobalLifetimeScope Configure");

            builder.RegisterComponentInHierarchy<UIManager>()
                .AsSelf();

            builder.Register<ResourceManager>(VContainer.Lifetime.Singleton).AsSelf();
            builder.Register<AddressableManager>(VContainer.Lifetime.Singleton).AsSelf();
            
            builder.RegisterComponentInHierarchy<CameraManager>().As<ICameraManager>();
            builder.RegisterComponentInHierarchy<InputManager>().As<IInputManager>();
            builder.RegisterEntryPoint<Entities.Character>().As<ICharacterManager>();
            builder.RegisterComponentInHierarchy<FieldManager>().As<IFieldManager>();
            builder.RegisterComponentInHierarchy<Party>().As<IParty>();
            builder.RegisterEntryPoint<BattleManager>(VContainer.Lifetime.Singleton).As<IBattleManager>();
            
            //builder.RegisterEntryPoint<Character>(VContainer.Lifetime.Singleton).AsSelf();
            builder.Register<ObjectPooler>(VContainer.Lifetime.Singleton).AsSelf();

            builder.RegisterComponentInHierarchy<SceneInitializer>()
                .AsSelf();
        }

        //private void RegisterUIManager(IContainerBuilder builder)
        //{
        //    var gameObj = GameObject.Instantiate(uiManagerPrefab);
        //    var uiManager = gameObj.GetComponent<UIManager>();

        //    builder.RegisterComponent(uiManager)
        //        .AsSelf();
        //}

        protected override void Awake()
        {
            base.Awake();

            Debug.Log("GlobalLifetimeScope Awake");
            DontDestroyOnLoad(this);

            InitalizeAsync().Forget();
        }

        private async UniTask InitalizeAsync()
        {
            await Container.Resolve<ResourceManager>()
                .InitializeAsync();

            await Container.Resolve<UIManager>()
               .InitializeAsync(Container);
            
            await Container.Resolve<ICharacterManager>()
                .InitializeAsync(Container);

            await Container.Resolve<IFieldManager>()
               .InitializeAsync(Container);

            await Container.Resolve<IBattleManager>()
                .InitializeAsync(Container);
            
            var sceneInitializer = Container.Resolve<SceneInitializer>();
            // var lifetimeScope = sceneInitializer.GetComponent<LifetimeScope>();
            await sceneInitializer
                .InitializeAsync(this);
        }
    }
}

