using System;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Creature.Action
{
    public class Knockback : Act<Knockback.Param>
    {
        public class Param : ActParam
        {
            public ICombatant Attacker { get; private set; } = null;
            public ICombatant Target { get; private set; } = null;

            public float KnockbackDistance { get; private set; } = 1f;

            public Param WithAttacker(ICombatant attacker)
            {
                Attacker = attacker;
                return this;
            }

            public Param WithTarget(ICombatant target)
            {
                Target = target;
                return this;
            }

            public Param WithKnockbackDistance(float knockbackDistance)
            {
                KnockbackDistance = knockbackDistance;
                return this;
            }
        }
        
        public override void Execute()
        {
            if (_param == null)
                return;

            KnockbackAsync().Forget();
        }

        private async UniTask KnockbackAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            var attacker = _param?.Attacker;
            if (attacker == null)
                return;

            var target = _param?.Target;
            if (target == null ||
               !target.IActor.IsAlive)
                return;

            var distance = _param.KnockbackDistance;
            var direction = (target.Transform.position - attacker.Transform.position).normalized;
            var targetPosition = target.Transform.position + direction * distance;

            // DoTween으로 이동
            await target.Transform.DOMove(targetPosition, distance * 0.05f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => { });
        }
    }
}