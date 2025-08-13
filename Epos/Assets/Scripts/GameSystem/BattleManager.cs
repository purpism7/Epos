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
using VContainer;
using Field = Battle.Field;
using VContainer.Unity;

namespace GameSystem
{
    public interface IBattleManager : IGeneric
    {
        void BeginTurnBased(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm);
        void BeginRealTime(PartyLocation allyPartyLocation, WayPoint[] wayPoints);
        // void Begin<T, V>(V data = null) where T : Battle.BattleType, new() where V : BattleType<V>.BaseData;
    }
    
    public class BattleManager : IBattleManager, BattleType.IListener, ITickable
    {
        [Inject] private ICharacterManager _iCharacterManager = null;

        private IObjectResolver _container = null;
        private Dictionary<System.Type, BattleType> _battleTypeDic = null;
        private Battle.BattleType _currBattleType = null;

        async UniTask IGeneric.InitializeAsync(IObjectResolver container)
        {
            _container = container;

            await UniTask.CompletedTask;
        }
        
        private void Begin<T, V>(V param = null) where T : Battle.BattleType, new() where V : BattleType<V>.BattleTypeParam
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
            
            (battleType as BattleType<V>)?.Initialize(param);
            battleType.Begin();

            _currBattleType = battleType;
        }

        #region ITickable
        void ITickable.Tick()
        {
            _currBattleType?.ChainUpdate();
        }
        #endregion
        
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

            var allyICombatantList = await SetAllyICombatantsAsync(allyParty, allyPartyLocation, -100f);
            battleModeData.AllyICombatantList?.AddRange(allyICombatantList);
            
            var battleMode = new BattleModeCreator<TurnBased, TurnBased.Data>()
                .SetData(battleModeData)
                .Create();
            
            var allyFieldParam = new Battle.Step.Party.Param
            {
                PartyLocation = allyPartyLocation,
            }.WithParty(allyParty);

            var enemyFieldParam = new Battle.Step.Party.Param
            {
                PartyLocation = enemyPartyLocation,
            }.WithParty(enemyPartyLocation.EnmeyParty);
            
            var fieldParam = new Battle.Field.Param(allyFieldParam, enemyFieldParam)
            {
                BattleMode = battleMode,
                
                PreprocessingParam = new Preprocessing.FieldParam
                {
                    CameraZoomInPos = pointTm.position,
                    CameraZoomInEndAction = () =>
                    {

                    },
                },
            };
            
            Begin<Field, Battle.Field.Param>(fieldParam);
        }

        private async UniTask<List<ICombatant>> SetAllyICombatantsAsync(Datas.ScriptableObjects.Party party, PartyLocation partyLocation, float offsetX = 0)
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
                
                
                var hero = _iCharacterManager?.Create<Hero>(info.CharacterId, partyLocation.CharacterRootTm);
                await UniTask.WaitUntil(() => hero != null);
                
                var pos = partyLocation.GetPartyPosition(info.Position - 1);
                pos.x += offsetX;
                
                ICombatant iCombatant = hero;
                iCombatant?.SetPosition(pos);
                
                iCombatantList.Add(iCombatant);
            }

            return iCombatantList;
        }
        
        void IBattleManager.BeginRealTime(PartyLocation allyPartyLocation, WayPoint[] wayPoints)
        {
            BeginRealTimeAsync(allyPartyLocation, wayPoints).Forget();
        }

        private async UniTask BeginRealTimeAsync(PartyLocation allyPartyLocation, WayPoint[] wayPoints)
        {
            var allyParty = MainManager.Get<IParty>().GetParty(1);
            var partyInfo = allyParty?.PositionInfos;
            if (partyInfo.IsNullOrEmpty())
                return;
            
            var battleModeData = new RealTime.Data()
                .WithWayPoints(wayPoints);
            
            var battleMode = new BattleModeCreator<RealTime, RealTime.Data>()
                .SetData(battleModeData)
                .Create(_container);
            
            //_container?.Inject(battleMode);
            
            var allyICombatantList = await SetAllyICombatantsAsync(allyParty, allyPartyLocation);
            battleModeData.AllyICombatantList?.AddRange(allyICombatantList);

                     
            //var allyICombatantList = await SetAllyICombatantsAsync(allyParty, allyPartyLocation);
            //battleModeData.EnemyICombatantList?.AddRange(monsters);

            var allyFieldParam = new Battle.Step.Party.Param
            {
                PartyLocation = allyPartyLocation,
            }.WithParty(allyParty);

            var fieldParam = new Battle.Field.Param(allyFieldParam, null)
            {
                BattleMode = battleMode,
                
                
            };
            
            Begin<Field, Field.Param>(fieldParam);
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
        #endregion

        #region BattleType.IListener
        void BattleType.IListener.End()
        {
            Debug.Log("End Battle");
        }
        #endregion
    }
}

