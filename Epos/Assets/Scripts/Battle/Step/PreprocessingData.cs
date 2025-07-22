using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Battle.Step
{
    public partial class Preprocessing : BattleStep<Preprocessing.Param>
    {
        public class FieldParam : Preprocessing.Param
        {
            public Vector3 CameraZoomInPos;
            public System.Action CameraZoomInEndAction = null;
            
            public FieldParam() : base(typeof(Field))
            {
                
            }
        }
    }
}

