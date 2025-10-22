using Battle;
using Battle.RealTime;
using Common;
using Creator;
using Creature;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using Parts;
using System.Collections.Generic;
using Scene;
using Entities;
using GameSystem;

namespace Scene
{
    public class RealTimeField : BaseField
    {
        [SerializeField]
        private PartyLocation partyLocation = null;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPoint<GameSystem.StrategyManager>(VContainer.Lifetime.Scoped)
                .As<IStrategyManager>();
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            //await UniTask.Yield();

            partyLocation?.Initialize();

            //var container = _fieldScope?.Container;
            var iFieldManaver = _lifetimeScope?.Container?.Resolve<IFieldManager>();
            var iBattleManager = _lifetimeScope?.Container?.Resolve<IBattleManager>();

            var waypoints = iFieldManaver?.IField?.GetFieldPoint<IRealTimeFieldPoint>()?.Waypoints;

            iBattleManager?.BeginRealTime(partyLocation, waypoints);
            //MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
        }
    }
}
