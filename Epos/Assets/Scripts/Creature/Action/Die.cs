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
            PlayAnimation(_param.AnimationKey, false);

            // Eff_MonsterDead_01 이펙트 실행
            var effectParam = new Effect.Param()
                .WithRootTm(_iActor?.Transform)
                .WithReturnParent(true);

            _iActor?.IEffectCtr?.Activate("Eff_MonsterDead_01", effectParam);

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
