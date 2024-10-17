using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using Battle.Mode;
using Battle.Step;


namespace Battle
{
    public class Field : BattleType<Field.Data>
    {
        public class Data : BaseData
        {
            public Preprocessing.FieldData PreprocessingData = null;
            public Deploy.FieldData LeftDeployData = null;
            public Deploy.FieldData RightDeployData = null;
        }
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            AddStep<Step.Preprocessing>(data.PreprocessingData);
            AddStep<Step.EnemyDeploy>(data.RightDeployData);
            AddStep<Step.AllyDeploy>(data.LeftDeployData);
            AddStep<Step.BattleStart>(isLast: true);
        }
    }
}

