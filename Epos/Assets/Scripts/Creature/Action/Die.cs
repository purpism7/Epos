using Cysharp.Threading.Tasks;
using Spine;
using UnityEngine;

namespace Creature.Action
{
    public class Die : Act<Die.Data>
    {
        public class Data : BaseData
        {
            
        }

        public override void Execute()
        {
            //SetAnimation(_data?.AnimationKey, false);

            _iActor?.Deactivate();
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
