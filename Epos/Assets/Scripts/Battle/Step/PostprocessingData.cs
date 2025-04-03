using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Step
{
    public partial class Postprocessing : BattleStep<Postprocessing.Data>
    {
        public class FieldData : Data
        {
            public System.Action CameraZoomOutEndAction = null;
            
            public FieldData() : base(typeof(Field))
            {
                
            }
        }
    }
}


