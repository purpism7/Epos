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
using Creature.Action;

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

            // 캐릭터당 1개씩 필요하므로 Transient (주입 시마다 새 인스턴스)
            builder.Register<ActController>(VContainer.Lifetime.Transient).As<IActController>();
            builder.Register<CreatureEffectController>(VContainer.Lifetime.Transient).As<ICreatureEffectController>();
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

