using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

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
            public Forces.FieldData LeftForcesData = null;
            public Forces.FieldData RightForcesData = null;
        }
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            AddStep<Step.Preprocessing>(_data.PreprocessingData);
            AddStep<Step.EnemyForces>(_data.RightForcesData);
            AddStep<Step.AllyForces>(_data.LeftForcesData);
            AddStep<Step.BattleStart>(
                new BattleStart.Data
                {
                    LeftForces = _data?.LeftForcesData?.Forces,
                    RightForces = _data?.RightForcesData?.Forces,
                }, isLast: true);
        }

        protected override void End()
        {
            base.End();
            
            AddStep<BattleResult>();
            AddStep<Step.EnemyForces>(_data?.RightForcesData?.SetBattleState(false));
            AddStep<Step.AllyForces>(_data?.LeftForcesData?.SetBattleState(false));
            AddStep<BattleEnd>(
                new BattleEnd.Data
                {
                    EndAction = BattleEnd,
                });
            
            AddStep<Postprocessing>(new Postprocessing.FieldData(), isLast: true);
            
            Begin();
        }   
    }
}

