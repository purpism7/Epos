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
            public Data(BattleMode mode) : base(mode)
            {
                
            }
            
            public Preprocessing.FieldData PreprocessingData = null;
            public Deploy.FieldData LeftDeployData = null;
            public Deploy.FieldData RightDeployData = null;
        }

        public new interface IListener
        {
            
        }
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            AddStep<Step.Preprocessing>(data?.PreprocessingData);
            AddStep<Step.EnemyDeploy>(data?.LeftDeployData);
            AddStep<Step.AllyDeploy>(data?.RightDeployData);
            AddStep<Step.BattleStart>();
        }

        public override void Begin()
        {
            base.Begin();
        }
    }
}

