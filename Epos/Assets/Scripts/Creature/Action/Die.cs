using Cysharp.Threading.Tasks;
using Spine;
using System;
using UnityEngine;

namespace Creature.Action
{
    public class Die : Act<Die.Param>
    {
        public class Param : ActParam
        {
            
        }

        public override void Execute()
        {
            PlayAnimation("Damege", false);

            // _iActor?.Deactivate();
            DeactivateAsync().Forget();
        }

        private async UniTask DeactivateAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            _iActor?.Deactivate();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);
            
            _iActor?.Deactivate();
        }
    }
}
