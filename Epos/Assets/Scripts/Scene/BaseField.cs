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
        protected LifetimeScope _fieldScope = null;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<BattleManager>(VContainer.Lifetime.Scoped).As<IBattleManager>();
            builder.RegisterEntryPoint<FieldManager>(VContainer.Lifetime.Scoped).As<IFieldManager>();
            builder.RegisterEntryPoint<CombatantCreator>(VContainer.Lifetime.Scoped).AsSelf();
            builder.Register<WeakTypeMap<IActor>>(VContainer.Lifetime.Scoped).AsSelf();
        }

        public override async UniTask InitializeAsync(LifetimeScope parentLifetimeScope)
        {
            await base.InitializeAsync(parentLifetimeScope);

            _fieldScope = parentLifetimeScope.CreateChild(Configure);
            await UniTask.Yield();

            var container = _fieldScope?.Container;
            await container.Resolve<IBattleManager>().InitializeAsync();
            await container.Resolve<IFieldManager>().InitializeAsync();

        }
    }
}

