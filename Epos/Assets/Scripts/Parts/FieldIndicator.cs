using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Parts
{
    public class FieldIndicator : Part<FieldIndicator.Param>
    {
        public class Param : Common.Param
        {
            public Vector3 TargetPos = Vector3.zero;
        }

        public override void Activate(Param param)
        {
            if (param == null)
                return;

            transform.position = param.TargetPos;
            
            base.Activate(param);
        }
    }
}

