using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

using DG.Tweening;

namespace Creature.Action
{
    public class Knockback : Act<Knockback.Param>
    {
        public class Param : ActParam
        {
            public Vector3? AttackerPosition { get; private set; } = null;
            public Transform TargetTransform { get; private set; } = null;
            public float KnockbackDistance { get; private set; } = 1f;

            public Param WithAttackerPosition(Vector3 attackerPosition)
            {
                AttackerPosition = attackerPosition;
                return this;
            }

            public Param WithTargetTransform(Transform targetTransform)
            {
                TargetTransform = targetTransform;
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

            var knockbackDistance = _param.KnockbackDistance;
            var direction = (_param.TargetTransform.position - _param.AttackerPosition.Value).normalized;
            var targetPosition = _param.TargetTransform.position + direction * knockbackDistance;

            // DoTween으로 이동
            _param.TargetTransform.DOMove(targetPosition, knockbackDistance * 0.05f)
                .SetEase(Ease.OutQuad)
                .OnComplete(End);
        }
    }
}