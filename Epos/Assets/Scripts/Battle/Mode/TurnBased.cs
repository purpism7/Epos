using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Ability;
using Creature;
using Creature.Action;

namespace Battle.Mode
{
    public class TurnBased : BattleMode<TurnBased.Data>, Casting.IListener
    {
        public class Data : BaseData
        {
            public EType EType = EType.None;
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
        
        private List<IActController> _sequenceActList = null;
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
                        
                        iCombatant.SetETeam(Type.ETeam.Ally);
                        _priorityICombatantList?.Add(iCombatant);
                    }
                    
                    foreach (var iCombatant in _data?.EnemyICombatantList)
                    {
                        if (iCombatant == null)
                            continue;
                        
                        iCombatant.SetETeam(Type.ETeam.Enemy);
                        _priorityICombatantList?.Add(iCombatant);
                    }
                    
                    // _priorityICombatantList?.AddRange(_data.AllyICombatantList);
                    // _priorityICombatantList?.AddRange(_data.EnemyICombatantList);

                    // 임시로 initialize 진행. 차후 덱 편성 후 캐릭터 생성 시 진행 예정.
                    foreach (var iCombatant in _priorityICombatantList)
                    {
                        var character = iCombatant as Character;
                        if(character == null)
                            continue;
                        
                        character.Initialize();
                        character.Activate();
                    }
                    
                    _priorityICombatantList = _priorityICombatantList?.OrderByDescending(iActor => iActor?.IStat?.Get(Stat.EType.ActionSpeed)).ToList();
     
                    break;
                }
            }
            
            StartTurnAsync().Forget();
        }

        public override void ChainUpdate()
        {
            _iActCtr?.ChainUpdate();
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

                    if (iCombatant.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Active) == null)
                        continue;
                    
                    _castingActiveSkillICombatantQueue?.Enqueue(iCombatant);
                }
            }

            if (_castingActiveSkillICombatantQueue?.Count <= 0)
            {
                End();
                
                return;
            }
            
            SetTurn(_turn + 1);
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            CastingActiveSkillAsync().Forget();
        }

        private void EndTurn()
        {
            StartTurnAsync().Forget();
        }

        private void SetTurn(int turn)
        {
            _turn = turn;
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
            
            var activeSkill = attacker?.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Active);
            if (activeSkill == null)
                return;
            
            var targetList = attacker.GetTargetList(_priorityICombatantList, activeSkill);
            if (targetList != null)
            {
                foreach (var target in targetList)
                {
                    if(target == null)
                        continue;

                    _targetDataList.Add(
                        new TargetData
                        {
                            Target = target,
                        });
                }
            }
            
            await CastingPassiveSkillAsync(attacker);
            
            MoveToTarget(attacker, activeSkill, _targetDataList);
            CastingSkill(attacker, activeSkill, _targetDataList);
            // attacker.IActCtr?.CastingSkill(this, activeSkill, _targetList); 

            _sequenceActList?.Add(attacker.IActCtr);
            
            UpdateActAsync().Forget();
        }
        
        private async UniTask UpdateActAsync()
        {
            await SequenceActAsync();

            // Debug.Log("End");
            await MoveToReturnAsync();
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            // Debug.Log("End Move To Return");

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
                    _iActCtr = _sequenceActList[_sequenceIndex];
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
                foreach (var iActCtr in _sequenceActList)
                {
                    if(iActCtr == null)
                        continue;
                    
                    iActCtr.MoveToTarget()?.Execute();
                    
                    SetSortingOrder(iActCtr as ICombatant, 0);
                }

                while (_sequenceActList?.Find(iActCtr => iActCtr.InAction) != null)
                {
                    foreach (var iActCtr in _sequenceActList)
                    {
                        if (iActCtr.InAction)
                            iActCtr.ChainUpdate();
                    }
                    
                    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                }
            }
        }

        private async UniTask CastingPassiveSkillAsync(ICombatant attacker)
        {
            if (_priorityICombatantList == null)
                return;
            
            foreach (var iCombatant in _priorityICombatantList)
            {
                if(iCombatant == null)
                    continue;

                var passiveSkill = iCombatant.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Passive);
                if(passiveSkill == null)
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
                        if (passiveSkill.SameTeam)
                        {
                            if(attacker.ETeam == iCombatant.ETeam)
                                continue;
                                
                            var findTargetData = _targetDataList?.Find(targetData => targetData?.Target.Id == target.Id);
                            if (findTargetData != null)
                            {
                                // _targetList.RemoveAt(findIndex);
                                findTargetData.SetChangeTarget(iCombatant);
                            }
                            
                            MoveToTarget(iCombatant, passiveSkill, targetDataList);
                            CastingSkill(iCombatant, passiveSkill, targetDataList);
                            // iCombatant.IActCtr?.CastingSkill(this, passiveSkill, targetList); 
                                
                            _sequenceActList?.Add(iCombatant.IActCtr);
                                    
                            continue;
                        }
                    }
                }
                        
                MoveToTarget(iCombatant, passiveSkill, targetDataList);
                CastingSkill(iCombatant, passiveSkill, targetDataList);
                // iCombatant.IActCtr?.CastingSkill(this, passiveSkill, targetList); 
                            
                _sequenceActList?.Add(iCombatant.IActCtr);
            }

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        }

        private void MoveToTarget(ICombatant attacker, Skill skill, List<TargetData> targetDataList)
        {
            if (skill == null)
                return;

            if (targetDataList == null ||
                targetDataList.Count > 1)
                return;

            var targetData = targetDataList.FirstOrDefault();
            if (targetData == null)
                return;
            
            var skillRange = skill.SkillData.Range;
            if (skillRange <= 0)
                return;

            var target = targetData.Target;
            if (targetData.ChangeTarget != null)
            {
                skillRange += 2f;
            }
            
            var targetPos = target.Transform.position;
            var direction = targetPos.x - attacker.Transform.position.x;
            
            targetPos.x = direction <= 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
            targetPos.y -= 1f;
            targetPos.z = 0;

            SetSortingOrder(attacker, 1);
            
            attacker.IActCtr?.MoveToTarget(targetPos, reverse: targetPos.x - attacker.Transform.position.x < 0);
        }

        private void CastingSkill(ICombatant attacker, Skill skill, List<TargetData> targetDataList)
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
            
            attacker.IActCtr?.CastingSkill(this, skill, targetList); 
        }
        
        private void SetSortingOrder(ICombatant iCombatant, int sortingOrder)
        {
            var meshRenderer = iCombatant?.SkeletonAnimation?.GetComponent<MeshRenderer>();
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

        void Casting.IListener.AfterCasting()
        {
            
        }
        #endregion
    }
}

