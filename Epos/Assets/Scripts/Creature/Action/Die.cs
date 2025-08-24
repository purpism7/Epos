using Cysharp.Threading.Tasks;
using Spine;
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
            SetAnimation("Damege", false);

            // _iActor?.Deactivate();
        }


        //private async UniTask DeactivateAsync()
        //{

        //}
        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);
            
            _iActor?.Deactivate();
        }
    }
}
