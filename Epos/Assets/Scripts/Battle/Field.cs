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
            public Party.FieldData AllyFieldData { get; private set; } = null;
            public Party.FieldData EnemyFieldData { get; private set; } = null;

            public Data(Party.FieldData allyFieldData, Party.FieldData enemyFieldData)
            {
                AllyFieldData = allyFieldData;
                EnemyFieldData = enemyFieldData;
            }
        }
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            if(_data?.PreprocessingData != null)
                AddStep<Step.Preprocessing>(_data?.PreprocessingData);

            if (_data?.EnemyFieldData != null)
                AddStep<Step.EnemyParty>(_data?.EnemyFieldData);

            if (_data?.AllyFieldData != null)
                AddStep<Step.AllyParty>(_data?.AllyFieldData);

            AddStep<Step.BattleStart>(
                new BattleStart.Data(_data?.AllyFieldData?.Party, _data?.EnemyFieldData?.Party), 
                isLast: true);
        }

        protected override void End()
        {
            base.End();
            
            AddStep<BattleResult>();
           
            if (_data?.EnemyFieldData != null)
                AddStep<Step.EnemyParty>(_data?.EnemyFieldData?.SetBattleState(false));

            if (_data?.AllyFieldData != null)
                AddStep<Step.AllyParty>(_data?.AllyFieldData?.SetBattleState(false));

            AddStep<BattleEnd>(
                new BattleEnd.Data
                {
                    EndAction = BattleEnd,
                }, _data?.PreprocessingData == null);

            if (_data?.PreprocessingData != null)
                AddStep<Postprocessing>(new Postprocessing.FieldData(), isLast: true);
            
            Begin();
        }   
    }
}

