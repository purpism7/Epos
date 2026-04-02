using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using Battle;
using Battle.RealTime;
using Common;
using Creator;
using Creature;
using Parts;
using Entities;
using GameSystem;
using Battle.Strategy;
using UI.Presenter;

namespace Scene
{
    public class RealTimeField : BaseField
    {
        [SerializeField]
        private PartyLocation partyLocation = null;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<StrategyController>(VContainer.Lifetime.Scoped)
                .As<IStrategyController>();

            builder.Register<ItemManager>(VContainer.Lifetime.Scoped).AsSelf();
            builder.Register<BattleMainPresenter>(VContainer.Lifetime.Scoped).AsSelf().As<IBattleMainPresenter>();
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            
            var container = _lifetimeScope?.Container;
            
            var itemManager = container.Resolve<ItemManager>();
            var fieldManager = container?.Resolve<IFieldManager>();
            var battleManager = container?.Resolve<IBattleManager>();

            if(itemManager != null)
                await itemManager.InitializeAsync();
            
            partyLocation?.Initialize();

            var waypoints = fieldManager?.IField?.GetFieldPoint<IRealTimeFieldPoint>()?.Waypoints;
            battleManager?.BeginRealTime(partyLocation, waypoints);
            
            //MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
        }
    }
}
