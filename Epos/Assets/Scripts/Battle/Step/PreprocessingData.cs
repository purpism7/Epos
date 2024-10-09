using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Battle.Step
{
    public partial class Preprocessing : BattleStep<Preprocessing.Data>
    {
        public class FieldData : Data
        {
            public Vector3 CameraZoomInPos;
            public System.Action CameraZoomInEndAction = null;
            
            public FieldData() : base(typeof(Field))
            {
                
            }
        }
    }
}

