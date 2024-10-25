using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;

namespace Creature.Action
{
    public class Casting : Act<Casting.Data>
    {
        public class Data : BaseData
        {
            public IListener IListener = null;
        }

        public interface IListener
        {
            void BeforeCasting();
            void InUse();
            void AfterCasting();
        }

        private ICastingController _iCastingCtr = null;

        public override void Initialize(IActor iActor)
        {
            base.Initialize(iActor);

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;

            _iCastingCtr = iActorTm.AddOrGetComponent<CastingController>();
            _iCastingCtr?.Initialize(_iActor as ICaster);
        }

        public override void Execute()
        {
            if (_data == null)
                return;

        }
    }
}

