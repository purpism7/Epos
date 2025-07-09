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
            
            if(_data?.PreprocessingData != null)
                AddStep<Step.Preprocessing>(_data?.PreprocessingData);

            if (_data?.EnemyPartyData != null)
                AddStep<Step.EnemyParty>(_data?.EnemyPartyData);

            if (_data?.AllyPartyData != null)
                AddStep<Step.AllyParty>(_data?.AllyPartyData);

            AddStep<Step.BattleStart>(
                new BattleStart.Data(_data?.AllyPartyData?.Party, _data?.EnemyPartyData?.Party), 
                isLast: true);
        }

        protected override void End()
        {
            base.End();
            
            AddStep<BattleResult>();
           
            if (_data?.EnemyPartyData != null)
                AddStep<Step.EnemyParty>(_data?.EnemyPartyData?.SetBattleState(false));

            if (_data?.AllyPartyData != null)
                AddStep<Step.AllyParty>(_data?.AllyPartyData?.SetBattleState(false));

            AddStep<BattleEnd>(
                new BattleEnd.Data
                {
                    EndAction = BattleEnd,
                }, _data.PreprocessingData == null);

            if (_data?.PreprocessingData != null)
                AddStep<Postprocessing>(new Postprocessing.FieldData(), isLast: true);
            
            Begin();
        }   
    }
}

