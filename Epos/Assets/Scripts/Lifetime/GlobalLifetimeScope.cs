using UnityEngine;

using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

using Creator;
using Creature;
using Entities;
using GameSystem;
using Scene;
using System;


namespace Lifetime
{

    public class GlobalLifetimeScope : LifetimeScope
    {
        //[SerializeField] private GameObject uiManagerPrefab = null;

        public class GenericResolver// : IInstanceProvider
        {
            readonly Type implType;
            readonly VContainer.Lifetime lifetime;

            public GenericResolver(Type implType, VContainer.Lifetime lifetime)
            {
                this.implType = implType;
                this.lifetime = lifetime;
            }

            public object CreateInstance(IObjectResolver resolver, Type type)
            {
                var genericType = implType.MakeGenericType(type.GetGenericArguments());
                return Activator.CreateInstance(genericType);
            }
        }

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

            builder.RegisterComponentInHierarchy<SceneInitializer>().AsSelf();

            builder.Register(typeof(UICreator<,>), VContainer.Lifetime.Transient).AsSelf();
            builder.Register<UIFactory>(VContainer.Lifetime.Singleton);
            //builder.RegisterFactory<Type, object>(c => 
            //    (Type type) =>
            //    {
            //        var repoType = typeof(UICreator).MakeGenericType(type);
            //        return Activator.CreateInstance(repoType);
            //    }, VContainer.Lifetime.Singleton);
            //builder.Register(typeof(Creator<>), VContainer.Lifetime.Singleton).AsSelf();
            //builder.Register(new GenericResolver(typeof(UICreator<>), VContainer.Lifetime.Singleton));
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

