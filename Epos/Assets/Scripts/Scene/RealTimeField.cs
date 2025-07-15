using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

using Creature;
using Battle;
using Entities;
using Scene;
using GameSystem;
using Parts;

namespace Scene
{
    public class RealTimeField : SceneInitializer
    {
        [SerializeField]
        private PartyLocation partyLocation = null;
        [SerializeField]
        private WayPoint[] wayPoints = null;

        //protected override void Configure(IContainerBuilder builder)
        //{
        //    base.Configure(builder);

        //    Debug.Log("RealTimeField Configure");
        //}

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);


        }

        public override async UniTask InitializeAsync(LifetimeScope parentLifetimeScope)
        {
            await base.InitializeAsync(parentLifetimeScope);

            foreach (var wayPoint in wayPoints)
            {
                wayPoint?.Initialize();
            }

            await UniTask.Yield();

            partyLocation?.Initialize();
            _iBattleManager?.BeginRealTime(partyLocation, wayPoints);
            //MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
        }
    }
}
