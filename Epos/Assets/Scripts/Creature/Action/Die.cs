using Cysharp.Threading.Tasks;
using Spine;
using System;
using Common;
using Item;
using UnityEngine;
using VContainer;

namespace Creature.Action
{
    public class Die : Act<Die.Param>
    {
        public class Param : ActParam
        {
            
        }

        [Inject] private ItemFactory _itemFactory = null;

        public override void Execute()
        {
            PlayAnimation(_param.AnimationKey, false);

            // Eff_MonsterDead_01 이펙트 실행
            if (_actor is Monster)
            {
                var effectParam = new Effect.Param()
                    .WithRootTm(_actor?.Transform)
                    .WithReturnParent(true);
                
                _actor?.EffectController?.Activate("Eff_MonsterDead_01", effectParam);
            }
            
            DeactivateAsync().Forget();
            
            Debug.Log(_itemFactory);
            var dropItem = _itemFactory?.Create<DropItem>(null);
            if (dropItem != null &&
                _actor != null)
            {
                var dropItemParam = new DropItem.Param(_actor.SortingOrder, _actor.Transform.position);
                dropItem.Activate(dropItemParam);
            }
  
        }

        private async UniTask DeactivateAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            _actor?.Deactivate();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);
            
            _actor?.Deactivate();
        }
    }
}
