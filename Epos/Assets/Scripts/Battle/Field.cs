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
            public Party.FieldData LeftPartyData = null;
            public Party.FieldData RightPartyData = null;
        }
        
        public override void Initialize(Data data)
        {
            base.Initialize(data);
            
            AddStep<Step.Preprocessing>(_data.PreprocessingData);
            AddStep<Step.EnemyParty>(_data.RightPartyData);
            AddStep<Step.AllyParty>(_data.LeftPartyData);
            AddStep<Step.BattleStart>(
                new BattleStart.Data
                {
                    Left = _data?.LeftPartyData?.PartyLocation,
                    Right = _data?.RightPartyData?.PartyLocation,
                }, isLast: true);
        }

        protected override void End()
        {
            base.End();
            
            AddStep<BattleResult>();
            AddStep<Step.EnemyParty>(_data?.RightPartyData?.SetBattleState(false));
            AddStep<Step.AllyParty>(_data?.LeftPartyData?.SetBattleState(false));
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

