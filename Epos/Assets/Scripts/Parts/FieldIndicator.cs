using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Battle;

namespace Parts
{
    public class FieldIndicator : Part<FieldIndicator.Param>
    {
        public class Param : Common.Param
        {
            public Vector3 TargetPos = Vector3.zero;
        }

        public override UniTask ActivateAsync(Param param)
        {
            if (param == null)
                return UniTask.CompletedTask;

            base.ActivateAsync(param);
            transform.position = param.TargetPos;
            
            return UniTask.CompletedTask;
        }
    }
}

