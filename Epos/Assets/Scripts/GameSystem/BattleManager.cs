using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle;
using Battle.Mode;
using Battle.Step;
using Common;
using Creature;
using Manager;
using Character = Manager.Character;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        void BeginFieldBattle(Parts.Deploy leftDeploy, Parts.Deploy rightDeploy, Transform pointTm);
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
                aBattleType?.Initialize(data);
                aBattleType?.SetIListener(this);
       
                _battleTypeDic?.TryAdd(typeof(T), aBattleType);

                battleType = aBattleType;
            }

            if (battleType == null)
            {
                Debug.Log("No Create = " + typeof(T));
                
                return;
            }
            
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
        /// <param name="leftDeploy">Ally</param>
        /// <param name="rightDeploy">Enemy</param>
        /// <param name="pointTm">For Zoom In Camera</param>
        void IBattleManager.BeginFieldBattle(Parts.Deploy leftDeploy, Parts.Deploy rightDeploy, Transform pointTm)
        {
            var battleModeData = new TurnBased.Data
            {
                AllyICombatantList = leftDeploy?.characters?.AddList<ICombatant, Creature.Character>(),
                EnemyICombatantList = rightDeploy?.characters?.AddList<ICombatant, Creature.Character>(),

                EType = TurnBased.EType.ActionSpeed,
            };
            
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

                LeftDeployData = new Battle.Step.Deploy.FieldData
                {
                    Deploy = leftDeploy,
                },

                RightDeployData = new Battle.Step.Deploy.FieldData
                {
                    Deploy = rightDeploy,
                },
            };
            
            Begin<Field, Battle.Field.Data>(fieldData);
        }
        #endregion
        
        #region BattleType.IListener
        void BattleType.IListener.End()
        {
            
        }
        #endregion
    }
}

