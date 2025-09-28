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
            // public Vector3? TargetPosition { get; private set; } = null;

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
            
            // public Param WithTargetPosition(Vector3? targetPosition)
            // {
            //     TargetPosition = targetPosition;
            //     return this;
            // }
        }
        
        public override void Execute()
        {
            if (_param == null)
                return;
         
            var direction = (_param.TargetTransform.position - _param.AttackerPosition.Value).normalized;

            // 목적지 계산
            var targetPosition = _param.TargetTransform.position + direction * 3f;

            // DoTween으로 이동
            _param.TargetTransform.DOMove(targetPosition, 1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // isKnockbacking = false;
                });
        }
    }
}