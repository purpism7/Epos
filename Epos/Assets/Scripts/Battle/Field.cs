using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using System.Data;
using Battle.Mode;
using Battle.Step;

namespace Battle
{
    public class Field : BattleType<Field.Param>
    {
        public class Param : BattleTypeParam
        {
            public Preprocessing.FieldParam PreprocessingParam = null;
            public Party.Param AllyFieldParam { get; private set; } = null;
            public Party.Param EnemyFieldParam { get; private set; } = null;

            public Param(Party.Param allyFieldParam, Party.Param enemyFieldParam)
            {
                AllyFieldParam = allyFieldParam;
                EnemyFieldParam = enemyFieldParam;
            }
        }
        
        public override void Initialize(Param param)
        {
            base.Initialize(param);
            
            if(_param?.PreprocessingParam != null)
                AddStep<Step.Preprocessing>(_param?.PreprocessingParam);

            if (_param?.EnemyFieldParam != null)
                AddStep<Step.EnemyParty>(_param?.EnemyFieldParam);

            if (_param?.AllyFieldParam != null)
                AddStep<Step.AllyParty>(_param?.AllyFieldParam);

            AddStep<Step.BattleStart>(
                new BattleStart.Param(_param?.AllyFieldParam?.Party, _param?.EnemyFieldParam?.Party), 
                isLast: true);
        }

        protected override void End()
        {
            base.End();
            
            AddStep<BattleResult>();
           
            if (_param?.EnemyFieldParam != null)
                AddStep<Step.EnemyParty>(_param?.EnemyFieldParam?.SetBattleState(false));

            if (_param?.AllyFieldParam != null)
                AddStep<Step.AllyParty>(_param?.AllyFieldParam?.SetBattleState(false));

            AddStep<BattleEnd>(
                new BattleEnd.Param
                {
                    EndAction = BattleEnd,
                }, _param?.PreprocessingParam == null);

            if (_param?.PreprocessingParam != null)
                AddStep<Postprocessing>(new Postprocessing.FieldParam(), isLast: true);
            
            Begin();
        }   
    }
}

