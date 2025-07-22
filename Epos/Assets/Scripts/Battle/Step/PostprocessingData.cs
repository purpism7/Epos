using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Step
{
    public partial class Postprocessing : BattleStep<Postprocessing.Param>
    {
        public class FieldParam : Param
        {
            public System.Action CameraZoomOutEndAction = null;
            
            public FieldParam() : base(typeof(Field))
            {
                
            }
        }
    }
}


