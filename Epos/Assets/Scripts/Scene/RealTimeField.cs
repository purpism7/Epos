using Battle;
using Battle.RealTime;
using Creature;
using Cysharp.Threading.Tasks;
using Entities;
using GameSystem;
using Parts;
using Scene;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scene
{
    public class RealTimeField : SceneInitializer
    {
        [SerializeField]
        private PartyLocation partyLocation = null;

        //protected override void Configure(IContainerBuilder builder)
        //{
        //    base.Configure(builder);

        //    Debug.Log("RealTimeField Configure");
        //}

        [Inject]
        private IFieldManager _iFieldManager = null;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);


        }

        public override async UniTask InitializeAsync(LifetimeScope parentLifetimeScope)
        {
            await base.InitializeAsync(parentLifetimeScope);
            await UniTask.Yield();

            partyLocation?.Initialize();

            var waypoints = _iFieldManager.IField?.GetFieldPoint<IRealTimeFieldPoint>()?.Waypoints;

            _iBattleManager?.BeginRealTime(partyLocation, waypoints);
            //MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
        }
    }
}
