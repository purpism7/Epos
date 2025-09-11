using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using Entities;
using GameSystem;
using Scene;
using Common;
using Creator;
using Creature;

namespace Scene
{
    public abstract class BaseField : SceneInitializer
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<BattleManager>(VContainer.Lifetime.Scoped).As<IBattleManager>();
            builder.RegisterEntryPoint<FieldManager>(VContainer.Lifetime.Scoped).As<IFieldManager>();
            builder.RegisterEntryPoint<CombatantCreator>(VContainer.Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<ProjectileCreator>(VContainer.Lifetime.Scoped).AsSelf();
            builder.Register<WeakTypeMap<IActor>>(VContainer.Lifetime.Scoped).AsSelf();
            //builder.RegisterComponentInHierarchy<Monster>().AsSelf();
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();

            //await UniTask.Yield();

            //var container = _fieldScope?.Container;
            await _lifetimeScope.Container.Resolve<IBattleManager>().InitializeAsync();
            await _lifetimeScope.Container.Resolve<IFieldManager>().InitializeAsync();

        }
    }
}

