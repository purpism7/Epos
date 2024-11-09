using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Parts
{
    public class FieldIndicator : Part<FieldIndicator.Data>
    {
        public class Data : BaseData
        {
            public Vector3 TargetPos = Vector3.zero;
        }

        public override void Activate(Data data)
        {
            if (data == null)
                return;

            transform.position = data.TargetPos;
            
            base.Activate(data);
        }
    }
}

