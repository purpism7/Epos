using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle;
using Battle.Mode;
using Battle.Step;
using Creature;
using Entities;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        void BeginFieldBattle(Parts.Forces leftForces, Parts.Forces rightForces, Transform pointTm);
        // void Begin<T, V>(V data = null) where T : Battle.BattleType, new() where V : BattleType<V>.BaseData;
    }
    
    public class BattleManager : IBattleManager, BattleType.IListener
    {
        private Dictionary<System.Type, BattleType> _battleTypeDic = null;
        private Battle.BattleType _currBattleType = null;

        // private Dictionary<System.Type, BattleMode> _battleModeDic = null;
        
        public IGeneric Initialize()
        {
            return this;
        }

        private void Begin<T, V>(V data = null) where T : Battle.BattleType, new() where V : BattleType<V>.BaseData
        {
            if (_currBattleType != null)
                return;
            
            if (_battleTypeDic == null)
            {
                _battleTypeDic = new();
                _battleTypeDic.Clear();
            }

            Battle.BattleType battleType = null;
            if (!_battleTypeDic.TryGetValue(typeof(T), out battleType))
            {
                var aBattleType = new T() as BattleType<V>;
                aBattleType?.SetIListener(this);
       
                _battleTypeDic?.TryAdd(typeof(T), aBattleType);

                battleType = aBattleType;
            }

            if (battleType == null)
            {
                Debug.Log("No Create = " + typeof(T));
                
                return;
            }
            
            (battleType as BattleType<V>)?.Initialize(data);
            battleType.Begin();

            _currBattleType = battleType;
        }

        public void ChainUpdate()
        {
            _currBattleType?.ChainUpdate();
        }

        public void ChainLateUpdate()
        {
            
        }

        #region IBattleManager
        /// <summary>
        /// Set Field Battle
        /// </summary>
        /// <param name="leftForces">Ally</param>
        /// <param name="rightForces">Enemy</param>
        /// <param name="pointTm">For Zoom In Camera</param>
        void IBattleManager.BeginFieldBattle(Parts.Forces leftForces, Parts.Forces rightForces, Transform pointTm)
        {
            if (leftForces?.CharacterList == null)
                return;
            
            if (rightForces?.CharacterList == null)
                return;
            
            var battleModeData = new TurnBased.Data
            {
                // AllyICombatantList = leftForces?.CharacterList,
                // EnemyICombatantList = rightForces?.characters?.AddList<ICombatant, Creature.Character>(),

                EType = TurnBased.EType.ActionSpeed,
            };
            
            battleModeData.AllyICombatantList?.AddRange(leftForces.CharacterList);
            battleModeData.EnemyICombatantList?.AddRange(rightForces.CharacterList);
            
            var battleMode = new BattleModeCreator<TurnBased, TurnBased.Data>()
                .SetData(battleModeData)
                .Create();
            
            var fieldData = new Battle.Field.Data
            {
                BattleMode = battleMode,
                
                PreprocessingData = new Preprocessing.FieldData
                {
                    CameraZoomInPos = pointTm.position,
                    CameraZoomInEndAction = () =>
                    {

                    },
                },

                LeftForcesData = new Battle.Step.Forces.FieldData
                {
                    Forces = leftForces,
                    
                },

                RightForcesData = new Battle.Step.Forces.FieldData
                {
                    Forces = rightForces,
                },
            };
            
            Begin<Field, Battle.Field.Data>(fieldData);
        }
        #endregion
        
        #region BattleType.IListener
        void BattleType.IListener.End()
        {
            Debug.Log("End Battle");
        }
        #endregion
    }
}

