using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Battle;
using Battle.Mode;
using Battle.Step;
using Creature;
using Entities;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        void BeginFieldBattle(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm);
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
        void IBattleManager.BeginFieldBattle(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm)
        {
            if (left == null)
                return;

            if (right == null)
                return;

            left.Initialize();
            right.Initialize();

            BeginFieldBattleAsync(left, right, pointTm).Forget();
        }

        private async UniTask BeginFieldBattleAsync(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm)
        {
            var party = MainManager.Get<IParty>().GetParty(1);
            var partyInfo = party?.PositionInfos;
            if (partyInfo.IsNullOrEmpty())
                return;
            
            var battleModeData = new TurnBased.Data
            {
                EType = TurnBased.EType.ActionSpeed,
            };

            var enemyICombatantList = await SetEnemyICombatantsAsync(right);
            battleModeData.EnemyICombatantList?.AddRange(enemyICombatantList);
            
            var allyICombatantList = await SetAllyICombatantsAsync(left);
            battleModeData.AllyICombatantList?.AddRange(allyICombatantList);
            
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

                LeftPartyData = new Battle.Step.Party.FieldData
                {
                    PartyLocation = left,
                }.WithParty(party),

                RightPartyData = new Battle.Step.Party.FieldData
                {
                    PartyLocation = right,
                },
            };
            
            Begin<Field, Battle.Field.Data>(fieldData);
        }

        private async UniTask<List<ICombatant>> SetAllyICombatantsAsync(Parts.PartyLocation left)
        {
            var partyInfo = MainManager.Get<IParty>().GetParty(1)?.PositionInfos;
            if (partyInfo.IsNullOrEmpty())
                return null;

            List<ICombatant> iCombatantList = new();
            iCombatantList.Clear();
            
            for (int i = 0; i < partyInfo?.Length; ++i)
            {
                var info = partyInfo[i];
                if(info == null)
                    continue;
                
                var hero = MainManager.Get<ICharacterManager>().Create<Hero>(info.CharacterId, left.CharacterRootTm);
                await UniTask.WaitUntil(() => hero != null);
                
                var pos = left.GetPartyPosition(info.Position - 1);
                pos.x -= 100f;
                
                ICombatant iCombatant = hero;
                iCombatant?.SetPosition(pos);
                
                // hero?.Initialize();
                // hero?.Deactivate();
  
                iCombatantList.Add(iCombatant);
            }

            return iCombatantList;
        }
        
        private async UniTask<List<ICombatant>> SetEnemyICombatantsAsync(Parts.PartyLocation right)
        {
            if (right.CharacterList.IsNullOrEmpty())
                return null;

            List<ICombatant> iCombatantList = new();
            iCombatantList.Clear();
            
            for (int i = 0; i < right.CharacterList.Count; ++i)
            {
                var characterId = right.CharacterList[i];
               
                var monster = MainManager.Get<ICharacterManager>().Create<Monster>(characterId, right.CharacterRootTm);
                await UniTask.WaitUntil(() => monster != null);
                monster?.Initialize();
                monster?.Activate();
                
                var pos = right.GetPartyPosition(monster.PartyPosition);
                
                ICombatant iCombatant = monster;
                iCombatant.SetPosition(pos);
                
                iCombatantList.Add(iCombatant);
            }

            return iCombatantList;
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

