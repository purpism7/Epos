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

        //[Inject]
        //private IFieldManager _iFieldManager = null;


        public override async UniTask InitializeAsync(LifetimeScope parentLifetimeScope)
        {
            await base.InitializeAsync(parentLifetimeScope);
            //await UniTask.Yield();

            partyLocation?.Initialize();

            var container = _fieldScope?.Container;
            var iFieldManaver = container?.Resolve<IFieldManager>();
            var iBattleManager = container?.Resolve<IBattleManager>();

            var waypoints = iFieldManaver?.IField?.GetFieldPoint<IRealTimeFieldPoint>()?.Waypoints;

            iBattleManager?.BeginRealTime(partyLocation, waypoints);
            //MainManager.Get<IBattleManager>()?.BeginRealTime(partyLocation, wayPoints);
        }
    }
}
