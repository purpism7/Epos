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
            public Party.FieldData AllyPartyData { get; private set; } = null;
            public Party.FieldData EnemyPartyData { get; private set; } = null;

            public Data(Party.FieldData allyPartyData, Party.FieldData enemyPartyData)
            {
                AllyPartyData = allyPartyData;
                EnemyPartyData = enemyPartyData;
            }
        }
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            AddStep<Step.Preprocessing>(_data.PreprocessingData);
            AddStep<Step.EnemyParty>(_data.EnemyPartyData);
            AddStep<Step.AllyParty>(_data.AllyPartyData);
            AddStep<Step.BattleStart>(
                new BattleStart.Data(_data?.AllyPartyData?.Party, _data?.EnemyPartyData?.Party), 
                isLast: true);
        }

        protected override void End()
        {
            base.End();
            
            AddStep<BattleResult>();
            AddStep<Step.EnemyParty>(_data?.EnemyPartyData?.SetBattleState(false));
            AddStep<Step.AllyParty>(_data?.AllyPartyData?.SetBattleState(false));
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

