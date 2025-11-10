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
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            //await UniTask.Yield();

            var container = _lifetimeScope?.Container;

            container?.Resolve<UIManager>()?.SetIObjectResolver(container);

            var iFieldManaver = container?.Resolve<IFieldManager>();
            var iBattleManager = container?.Resolve<IBattleManager>();

            partyLocation?.Initialize();

            var waypoints = iFieldManaver?.IField?.GetFieldPoint<IRealTimeFieldPoint>()?.Waypoints;
            iBattleManager?.BeginRealTime(partyLocation, waypoints);
            
            //MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
        }
    }
}
