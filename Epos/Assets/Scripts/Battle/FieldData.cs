using System.Collections;
using System.Collections.Generic;
using Battle.Step;
using UnityEngine;

namespace Battle
{
    public partial class Field : BattleType<Field.FieldData>
    {
        public class FieldData : Data
        {
            public Preprocessing.FieldData PreprocessingData = null;
            public Deploy.FieldData LeftDeployData = null;
            public Deploy.FieldData RightDeployData = null;
        }
        
    }
}

