using System.Collections;
using System.Collections.Generic;
using System.Data;
using Battle.Step;
using UnityEngine;

namespace Battle
{
    public partial class Field : BattleType<Field.FieldData>
    {
        public abstract class Data : BaseData
        {
            
        }

        public new interface IListener
        {
            
        }
        
        public override void Initialize(FieldData data)
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

