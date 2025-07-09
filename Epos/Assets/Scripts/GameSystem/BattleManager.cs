using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Battle;
using Battle.Mode;
using Battle.Step;
using Creature;
using Entities;
using Common;
using Parts;
using Field = Battle.Field;

namespace GameSystem
{
    public interface IBattleManager : IManager
    {
        void BeginTurnBased(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm);
        void BeginRealTime(Grid heroGrid);
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
        void IBattleManager.BeginTurnBased(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm)
        {
            if (left == null)
                return;

            if (right == null)
                return;

            left.Initialize();
            right.Initialize();

            BeginTurnBasedAsync(left, right, pointTm).Forget();
        }

        private async UniTask BeginTurnBasedAsync(Parts.PartyLocation allyPartyLocation, Parts.PartyLocation enemyPartyLocation, Transform pointTm)
        {
            var allyParty = MainManager.Get<IParty>().GetParty(1);
            var partyInfo = allyParty?.PositionInfos;
            if (partyInfo.IsNullOrEmpty())
                return;

            var battleModeData = new TurnBased.Data(TurnBased.EType.ActionSpeed);

            var enemyICombatantList = await SetEnemyICombatantsAsync(enemyPartyLocation);
            battleModeData.EnemyICombatantList?.AddRange(enemyICombatantList);

            var allyICombatantList = await SetAllyICombatantsAsync(allyParty, allyPartyLocation);
            battleModeData.AllyICombatantList?.AddRange(allyICombatantList);
            
            var battleMode = new BattleModeCreator<TurnBased, TurnBased.Data>()
                .SetData(battleModeData)
                .Create();
            
            var allyFieldData = new Battle.Step.Party.FieldData
            {
                PartyLocation = allyPartyLocation,
            }.WithParty(allyParty);

            var enemyFieldData = new Battle.Step.Party.FieldData
            {
                PartyLocation = enemyPartyLocation,
            }.WithParty(enemyPartyLocation.EnmeyParty);
            
            var fieldData = new Battle.Field.Data(allyFieldData, enemyFieldData)
            {
                BattleMode = battleMode,
                
                PreprocessingData = new Preprocessing.FieldData
                {
                    CameraZoomInPos = pointTm.position,
                    CameraZoomInEndAction = () =>
                    {

                    },
                },
            };
            
            Begin<Field, Battle.Field.Data>(fieldData);
        }

        private async UniTask<List<ICombatant>> SetAllyICombatantsAsync(Datas.ScriptableObjects.Party party, PartyLocation partyLocation)
        {
            var positionInfos = party?.PositionInfos;
            if (positionInfos.IsNullOrEmpty())
                return null;

            List<ICombatant> iCombatantList = new();
            iCombatantList.Clear();
            
            for (int i = 0; i < positionInfos?.Length; ++i)
            {
                var info = positionInfos[i];
                if(info == null)
                    continue;
                
                var hero = MainManager.Get<ICharacterManager>().Create<Hero>(info.CharacterId, partyLocation.CharacterRootTm);
                await UniTask.WaitUntil(() => hero != null);
                
                var pos = partyLocation.GetPartyPosition(info.Position - 1);
                pos.x -= 100f;
                
                ICombatant iCombatant = hero;
                iCombatant?.SetPosition(pos);
                
                iCombatantList.Add(iCombatant);
            }

            return iCombatantList;
        }
        
        private async UniTask<List<ICombatant>> SetEnemyICombatantsAsync(Parts.PartyLocation partyLocation)
        {
            // if (right.CharacterList.IsNullOrEmpty())
            //     return null;
            
            var positionInfos = partyLocation?.EnmeyParty?.PositionInfos;
            if (positionInfos.IsNullOrEmpty())
                return null;

            List<ICombatant> iCombatantList = new();
            iCombatantList.Clear();
            
            for (int i = 0; i < positionInfos?.Length; ++i)
            {
                var info = positionInfos[i];
                if(info == null)
                    continue;
               
                var monster = MainManager.Get<ICharacterManager>()?.Create<Monster>(info.CharacterId, partyLocation.CharacterRootTm);
                await UniTask.WaitUntil(() => monster != null);
                monster.Initialize();
                monster.Activate();
                
                var pos = partyLocation.GetPartyPosition(info.Position - 1);
                
                ICombatant iCombatant = monster;
                iCombatant.SetPosition(pos);
                
                iCombatantList.Add(iCombatant);
            }

            return iCombatantList;
        }

        void IBattleManager.BeginRealTime(Grid heroGrid)
        {
            var allyParty = MainManager.Get<IParty>().GetParty(1);
            var partyInfo = allyParty?.PositionInfos;
            if (partyInfo.IsNullOrEmpty())
                return;

            var heroICombatantList = new List<ICombatant>();
            heroICombatantList.Clear();

            for (int i = 0; i < partyInfo.Length; ++i)
            {
                var positionInfo = partyInfo[i];
                if (positionInfo == null)
                    continue;

                var hero = MainManager.Get<ICharacterManager>().Create<Creature.Hero>(positionInfo.CharacterId, heroGrid.transform.parent);
                if (hero == null)
                    continue;

                hero.Initialize();
                hero.Activate();

                heroICombatantList?.Add(hero);
            }

            //Begin<Battle.Mode.RealTime, Battle.Mode.RealTime.Data>();
            Begin<Field, Field.Data>();
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

