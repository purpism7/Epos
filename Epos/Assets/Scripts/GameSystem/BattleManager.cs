using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using Battle;
using Battle.Mode;
using Battle.Step;
using Creature;
using Entities;
using Common;
using Parts;
using Field = Battle.Field;
using Character = Creature.Character;
using Creator;

namespace GameSystem
{
    public interface IBattleManager : IGeneric
    {
        void BeginTurnBased(Parts.PartyLocation left, Parts.PartyLocation right, Transform pointTm);
        void BeginRealTime(PartyLocation allyPartyLocation, Waypoint[] waypoints);
        // void Begin<T, V>(V data = null) where T : Battle.BattleType, new() where V : BattleType<V>.BaseData;
    }
    
    public class BattleManager : IBattleManager, BattleType.IListener, ITickable, ILateTickable
    {
        [Inject] private IObjectResolver _iResolver = null;
        [Inject] private ICharacterManager _iCharacterManager = null;
        //[Inject] private ICameraManager _iCameraManager = null;
        [Inject] private IParty _iParty = null;

        private Dictionary<System.Type, BattleType> _battleTypeDic = null;
        private Battle.BattleType _currBattleType = null;
        private CombatantCreator _combatantCreator = null;

        async UniTask IGeneric.InitializeAsync()
        {
            _combatantCreator = _iResolver?.Resolve<CombatantCreator>();

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
                battleType = new T();
                
                var vBattleType = battleType as BattleType<V>;
                _iResolver?.Inject(vBattleType);

                vBattleType?.SetIListener(this);

                _battleTypeDic?.TryAdd(typeof(T), battleType);
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

        void ILateTickable.LateTick()
        {
            _currBattleType?.ChainLateUpdate();
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
            var allyParty = _iParty?.GetParty(1);
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
                .Create(_iResolver);
            
            var allyFieldParam = new Battle.Step.Party.Param
            {
                PartyLocation = allyPartyLocation,
            }.WithParty(allyParty);

            var enemyFieldParam = new Battle.Step.Party.Param
            {
                PartyLocation = enemyPartyLocation,
            }.WithParty(enemyPartyLocation.EnmeyParty);

            var fieldParam = new Battle.Field.Param(battleMode, allyFieldParam, null)
            {
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
                
                var hero = await _iCharacterManager.Create<Hero>(info.CharacterId, partyLocation.CharacterRootTm);
                
                ICombatant iCombatant = _combatantCreator?.Create(hero, hero?.Skills);
                // iCombatant?.SetPosition(pos);
                
                iCombatantList.Add(iCombatant);
            }

            return iCombatantList;
        }
        
        void IBattleManager.BeginRealTime(PartyLocation allyPartyLocation, Waypoint[] waypoints)
        {
            BeginRealTimeAsync(allyPartyLocation, waypoints).Forget();
        }

        private async UniTask BeginRealTimeAsync(PartyLocation allyPartyLocation, Waypoint[] waypoints)
        {
            var allyParty = _iParty?.GetParty(1);
            var partyInfo = allyParty?.PositionInfos;
            if (partyInfo.IsNullOrEmpty())
                return;
            
            var battleModeData = new RealTime.Data()
                .WithWayPoints(waypoints) ;
            
            var allyICombatantList = await SetAllyICombatantsAsync(allyParty, allyPartyLocation);
            battleModeData.AllyICombatantList?.AddRange(allyICombatantList);
            
            var battleMode = new BattleModeCreator<RealTime, RealTime.Data>()
                .SetData(battleModeData)
                .Create(_iResolver);

            var allyFieldParam = new Battle.Step.Party.Param
            {
                PartyLocation = allyPartyLocation,
            }.WithICombatantList(allyICombatantList);

            var fieldParam = new Battle.Field.Param(battleMode, allyFieldParam, null);
            
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

                var monster = await _iCharacterManager.Create<Monster>(info.CharacterId, partyLocation.CharacterRootTm);
                // var monster = _iCharacterManager?.Create<Monster>(info.CharacterId, partyLocation.CharacterRootTm);
                // await UniTask.WaitUntil(() => monster != null);
                monster.Initialize();
                monster.Activate();
                
                var pos = partyLocation.GetPartyPosition(info.Position - 1);
                
                ICombatant iCombatant = _combatantCreator?.Create(monster, monster.Skills);
                iCombatant?.SetPosition(pos);
                
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

