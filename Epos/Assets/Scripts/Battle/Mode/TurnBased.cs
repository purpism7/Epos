using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Datas.ScriptableObjects;
using Creature;
using Creature.Action;
using Common;
using GameSystem.Event;
using EventHandler = GameSystem.Event.EventHandler;

namespace Battle.Mode
{
    public class TurnBased : BattleMode<TurnBased.Data>, Casting.IListener
    {
        public class Data : BaseData
        {
            public EType EType { get; private set; } = EType.None;

            public Data(EType eType)
            {
                EType = eType;
            }
        }
        
        public enum EType
        {
            None,
            
            ActionSpeed,
        }

        private class TargetData
        {
            public ICombatant Target = null;
            public ICombatant ChangeTarget { get; private set; } = null;

            public void SetChangeTarget(ICombatant target)
            {
                ChangeTarget = target;
            }
        }

        private List<ICombatant> _priorityICombatantList = null;
        private Queue<ICombatant> _castingActiveSkillICombatantQueue = null;
        private List<TargetData> _targetDataList = null;
        
        private List<ICombatant> _sequenceActList = null;
        private IActController _iActCtr = null;

        private int _sequenceIndex = 0;
        private int _turn = 0;
        
        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);
            
            _castingActiveSkillICombatantQueue = new();
            _castingActiveSkillICombatantQueue.Clear();

            _sequenceActList = new();
            _targetDataList = new();
            
            return this;
        }

        /// <summary>
        /// 전투 시작.
        /// </summary>
        public override void Begin()
        {
            switch (_data.EType)
            {
                case EType.ActionSpeed:
                {
                    _priorityICombatantList = new();
                    _priorityICombatantList.Clear();

                    foreach (var iCombatant in _data?.AllyICombatantList)
                    {
                        if (iCombatant == null)
                            continue;
                        
                        iCombatant.SetETeam(ETeam.Ally);
                        _priorityICombatantList?.Add(iCombatant);
                        
                        EventHandler.Notify(new StatChangedEventData(iCombatant.IActor.Id, iCombatant.IActor.IStat));
                    }
                    
                    foreach (var iCombatant in _data?.EnemyICombatantList)
                    {
                        if (iCombatant == null)
                            continue;
                        
                        iCombatant.SetETeam(ETeam.Enemy);
                        iCombatant.IActor.IActCtr?.SetPosition(iCombatant.IActor.Transform.position);
                        _priorityICombatantList?.Add(iCombatant);
                        
                        EventHandler.Notify(new StatChangedEventData(iCombatant.IActor.Id, iCombatant.IActor.IStat));
                    }
                    
                    _priorityICombatantList = _priorityICombatantList?.OrderByDescending(iActor => iActor.IActor?.IStat?.Get(Stat.EType.ActionSpeed)).ToList();
     
                    break;
                }
            }

            AllyAppearanceAsync().Forget();
        }

        public override void ChainUpdate()
        {
            _iActCtr?.ChainUpdate();
        }

        public override void ChainLateUpdate()
        {
            
        }

        private async UniTask AllyAppearanceAsync()
        {
            var allyICombatantList = _data?.AllyICombatantList;
            if (allyICombatantList.IsNullOrEmpty())
                return;

            var sortAllyICombatantList = allyICombatantList; //?.OrderBy(iCombatant => iCombatant.PartyPosition).ToList();
            
            for (int i = 0; i < sortAllyICombatantList?.Count; ++i)
            {
                var iCombatant = sortAllyICombatantList[i];
                if(iCombatant == null)
                    continue;

                var hero = iCombatant as Hero;
                if(hero == null)
                    continue;
     
                hero.Activate();
                
                var originPos = iCombatant.IActor.Transform.position;
                originPos.x += 100f;

                var moveParam = new Move.Param
                {
                    MoveSpeed = 8f,
                    TargetPos = originPos,
                };
                iCombatant.IActor.IActCtr?.MoveToTargetPosition(moveParam)?
                    .Execute();
                
                await UniTask.WaitWhile(
                    () =>
                    {
                        hero.ChainUpdate();
                        return iCombatant.IActor.IActCtr.InAction;
                    });
                
                // 현재 위치 저장.
                iCombatant.IActor.IActCtr?.SetPosition(iCombatant.IActor.Transform.position);
            }

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            StartTurnAsync().Forget();
        }
        
        /// <summary>
        /// 턴 시작.
        /// </summary>
        private async UniTask StartTurnAsync()
        {
            _castingActiveSkillICombatantQueue?.Clear();
            
            if (_priorityICombatantList != null)
            {
                foreach (var iCombatant in _priorityICombatantList)
                {
                    if(iCombatant == null)
                        continue;
                    
                    // if(iCombatant.IStat?.Get(Stat.EType.ActivePoint) <= 0)
                    //     continue;

                    if (iCombatant.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active) == null)
                        continue;
                    
                    _castingActiveSkillICombatantQueue?.Enqueue(iCombatant);
                }
            }

            if (_castingActiveSkillICombatantQueue?.Count <= 0)
            {
                End(false);
                return;
            }
            
            SetTurn(_turn + 1);
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            CastingActiveSkillAsync().Forget();
        }

        private void EndTurn()
        {
            EventHandler.Notify<TurnBasedEventData>(new EndTurnEventData(_turn));
            
            StartTurnAsync().Forget();
        }

        private void SetTurn(int turn)
        {
            _turn = turn;
            
            EventHandler.Notify<TurnBasedEventData>(new StartTurnEventData(_turn));
            Debug.Log(turn);
        }
        
        private async UniTask CastingActiveSkillAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _sequenceIndex = 0;
            _sequenceActList?.Clear();
            _targetDataList?.Clear();
            
            ICombatant attacker = null;
            if (_castingActiveSkillICombatantQueue == null ||
                !_castingActiveSkillICombatantQueue.TryDequeue(out attacker))
                return;
            
            // 사용 가능 한 AcitveSkill 가져오기.
            var activeSkill = attacker?.ISkillCtr?.GetPossibleSkill(ESkillCategory.Active);
            if (activeSkill == null)
                return;
            
            // ActiveSkill 로 TargetList 가져오기. 
            var targetList = attacker.GetTargetList(_priorityICombatantList, activeSkill);
            if (targetList != null)
            {
                foreach (var target in targetList)
                {
                    if(target == null)
                        continue;

                    _targetDataList?.Add(
                        new TargetData
                        {
                            Target = target,
                        });
                }
            }
            
            await CastingPassiveSkillAsync(attacker);
            
            MoveToTarget(attacker, activeSkill, _targetDataList);
            CastingSkill(attacker, activeSkill, _targetDataList);

            _sequenceActList?.Add(attacker);
            
            UpdateActAsync().Forget();
        }
        
        private async UniTask UpdateActAsync()
        {
            await SequenceActAsync();
            await MoveToReturnAsync();
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            if (_castingActiveSkillICombatantQueue == null ||
                _castingActiveSkillICombatantQueue.Count <= 0)
            {
                EndTurn();

                return;
            }
            
            CastingActiveSkillAsync().Forget();
        }
        
        private async UniTask SequenceActAsync()
        {
            while (_sequenceActList != null &&
                   _sequenceActList.Count > _sequenceIndex)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                
                if (_iActCtr == null ||
                    !_iActCtr.InAction)
                {
                    _iActCtr = _sequenceActList[_sequenceIndex]?.IActor.IActCtr;
                    _iActCtr?.Execute();

                    ++_sequenceIndex;
                }
            }
            
            await UniTask.WaitWhile(() => _iActCtr != null && _iActCtr.InAction);
            
            _iActCtr = null;
        }

        private async UniTask MoveToReturnAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            if (_sequenceActList != null)
            {
                foreach (var iCombatant in _sequenceActList)
                {
                    if(iCombatant == null)
                        continue;

                    var moveParam = new Move.Param
                    {
                        MoveSpeed = iCombatant.IActor.IStat.Get(Stat.EType.MoveSpeed),
                        IsJumpMove = true,
                    };

                    iCombatant.IActor.IActCtr?
                        .MoveToTargetPosition(moveParam)?
                        .Execute();
                    
                    SetSortingOrder(iCombatant, 0);
                }

                while (_sequenceActList?.Find(iCombatant => iCombatant.IActor.IActCtr.InAction) != null)
                {
                    foreach (var iCombatant in _sequenceActList)
                    {
                        if(iCombatant?.IActor.IActCtr == null)
                            continue;
                        
                        if (iCombatant.IActor.IActCtr.InAction)
                            iCombatant.IActor.IActCtr.ChainUpdate();
                    }
                    
                    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                }
            }
        }

        // ActiveSkill 사용 전, 사용 가능한 PassiveSkill 가져오기.
        private async UniTask CastingPassiveSkillAsync(ICombatant attacker)
        {
            if (_priorityICombatantList == null)
                return;
            
            foreach (var iCombatant in _priorityICombatantList)
            {
                if(iCombatant == null)
                    continue;

                var passiveSkill = iCombatant.ISkillCtr?.GetPossibleSkill(ESkillCategory.Passive);
                if(passiveSkill == null)
                    continue;

                var skillData = passiveSkill?.SkillData;
                if (skillData == null)
                    continue;

                var targetList = iCombatant.GetTargetList(_priorityICombatantList, passiveSkill);
                if (targetList.IsNullOrEmpty())
                    continue;

                var targetDataList = new List<TargetData>();
                targetDataList.Clear();
                
                foreach (var target in targetList)
                {
                    if(target == null)
                        continue;

                    targetDataList.Add(
                        new TargetData
                        {
                            Target = target,
                        });
                }
                
                if (targetList.Count == 1)
                {
                    var target = targetList.FirstOrDefault();
                    if (target != null)
                    {
                        if (skillData.SameTeam)
                        {
                            if(attacker.ETeam == iCombatant.ETeam)
                                continue;
                                
                            var findTargetData = _targetDataList?.Find(targetData => targetData?.Target.IActor.Id == target.IActor.Id);
                            if (findTargetData != null)
                            {
                                // _targetList.RemoveAt(findIndex);
                                findTargetData.SetChangeTarget(iCombatant);
                            }
                            
                            MoveToTarget(iCombatant, passiveSkill, targetDataList);
                            CastingSkill(iCombatant, passiveSkill, targetDataList);
                            // iCombatant.IActCtr?.CastingSkill(this, passiveSkill, targetList); 
                                
                            _sequenceActList?.Add(iCombatant);
                                    
                            continue;
                        }
                    }
                }
                        
                MoveToTarget(iCombatant, passiveSkill, targetDataList);
                CastingSkill(iCombatant, passiveSkill, targetDataList);
                // iCombatant.IActCtr?.CastingSkill(this, passiveSkill, targetList); 
                            
                _sequenceActList?.Add(iCombatant);
            }

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        }

        // Target 에게 이동하여 ActiveSkill 사용하기.
        private void MoveToTarget(ICombatant attacker, Ability.ISkill iSkill, List<TargetData> targetDataList)
        {
            if (iSkill == null)
                return;

            var skillData = iSkill.SkillData;
            if(skillData == null)
                return;

            if (targetDataList == null ||
                targetDataList.Count > 1)
                return;

            var targetData = targetDataList.FirstOrDefault();
            if (targetData == null)
                return;
            
            var skillRange = skillData.Range;
            if(skillData.ESkillTarget != ESkillTarget.All)
            {
                if (skillRange <= 0)
                    return;
            }

            var target = targetData.Target;
            if (targetData.ChangeTarget != null)
                skillRange += 2f;
            
            var targetPos = target.IActor.Transform.position;
            var direction = targetPos.x - attacker.IActor.Transform.position.x;
            
            targetPos.x = direction <= 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
            targetPos.y -= 1f;
            targetPos.z = 0;

            SetSortingOrder(attacker, 1);

            var moveParam = new Move.Param
            {
                MoveSpeed = attacker.IActor.IStat.Get(Stat.EType.MoveSpeed),
                TargetPos = targetPos,
                IsJumpMove = true,
            };

            attacker.IActor?.IActCtr?.MoveToTargetPosition(moveParam);
        }

        private void CastingSkill(ICombatant attacker, Ability.ISkill iSkill, List<TargetData> targetDataList)
        {
            if (attacker == null)
                return;

            if (targetDataList.IsNullOrEmpty())
                return;

            var targetList = new List<ICombatant>();
            targetList.Clear();
            
            foreach (var targetData in targetDataList)
            {
                if(targetData == null)
                    continue;

                var target = targetData.ChangeTarget != null ? targetData.ChangeTarget : targetData.Target;
                targetList.Add(target);
            }

            var closestTarget = attacker?.FindClosestICombatant(targetList);

            attacker.IActor.IActCtr?.CastingSkill(this, attacker, iSkill, closestTarget, targetList);

            // if (skill.ESkillCategory == ESkillCategory.Active)
            {
                var eventData = new SkillUseEventData()
                    .WithISkill(iSkill)
                    .WithETeam(attacker.ETeam);
   
                GameSystem.Event.EventHandler.Notify(eventData);
            }
        }
        
        private void SetSortingOrder(ICombatant iCombatant, int sortingOrder)
        {
            var meshRenderer = iCombatant?.IActor.SkeletonAnimation?.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                return;
            
            meshRenderer.sortingOrder = sortingOrder;
        }
        
        #region Skill.IListener
        void Casting.IListener.BeforeCasting()
        {

        }

        void Casting.IListener.InUse()
        {
            
        }

        void Casting.IListener.AfterCasting(ICombatant iCombatant)
        {
            
        }
        #endregion
    }
}

