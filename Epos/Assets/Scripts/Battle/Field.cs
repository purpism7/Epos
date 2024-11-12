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
            
            AddStep<Step.Preprocessing>(_data.PreprocessingData);
            AddStep<Step.EnemyDeploy>(_data.RightDeployData);
            AddStep<Step.AllyDeploy>(_data.LeftDeployData);
            AddStep<Step.BattleStart>(isLast: true);
        }

        public override void End()
        {
            base.End();
        }
    }
}

