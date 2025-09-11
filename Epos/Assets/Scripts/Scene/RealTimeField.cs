using Battle;
using Battle.RealTime;
using Common;
using Creator;
using Creature;
using Cysharp.Threading.Tasks;
using Entities;
using GameSystem;
using Parts;
using Scene;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scene
{
    public class RealTimeField : BaseField
    {
        [SerializeField]
        private PartyLocation partyLocation = null;

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
