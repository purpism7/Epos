using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Ability;
using Creature;
using Creature.Action;
using Unity.VisualScripting;
using UnityEditor.Rendering;

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

        // private EType _eType = EType.None;
        
        private List<ICombatant> _priorityICombatantList = null;
        private Queue<ICombatant> _castingActiveSkillICombatantQueue = null;
        
        private List<IActController> _sequenceActList = null;
        
        
        
        private IActController _iActCtr = null;

        private int _sequencIndex = 0;
        // private ICombatant _currICombatant = null;
        
        private int _turn = 0;
        
        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);
            
            _castingActiveSkillICombatantQueue = new();
            _castingActiveSkillICombatantQueue.Clear();

            _sequenceActList = new();
            
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
            // _currICombatant?.IActCtr?.ChainUpdate();
        }
        
        /// <summary>
        /// 턴 시작.
        /// </summary>
        private async UniTask StartTurnAsync()
        {
            SetTurn(_turn + 1);
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _castingActiveSkillICombatantQueue?.Clear();
            
            if (_priorityICombatantList != null)
            {
                foreach (var iCombatant in _priorityICombatantList)
                {
                    if(iCombatant == null)
                        continue;
                    
                    _castingActiveSkillICombatantQueue?.Enqueue(iCombatant);
                }
            }
            
            CastingActiveSkillAsync().Forget();
        }

        private void EndTurn()
        {
            
        }

        private void SetTurn(int turn)
        {
            _turn = turn;
            Debug.Log(turn);
        }
        
        private async UniTask CastingActiveSkillAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            _sequencIndex = 0;
            _sequenceActList?.Clear();
            
            var attacker = _castingActiveSkillICombatantQueue?.Dequeue();
            if (attacker == null)
                return;
            
            var activeSkill = attacker.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Active);
            if (activeSkill == null)
                return;
            
            var targetList = GetTargetList(attacker, activeSkill);
            var target = targetList?.FirstOrDefault();
            
            await CastingPassiveSkillAsync(attacker, target);
            
            MoveToTarget(attacker, activeSkill, target);
            attacker.IActCtr?.CastingSkill(this, activeSkill, targetList); 

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
            CastingActiveSkillAsync().Forget();
        }
        
        private async UniTask SequenceActAsync()
        {
            while (_sequenceActList != null &&
                   _sequenceActList.Count > _sequencIndex)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                
                if (_iActCtr == null ||
                    !_iActCtr.InAction)
                {
                    _iActCtr = _sequenceActList[_sequencIndex];
                    _iActCtr?.Execute();

                    ++_sequencIndex;
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

        private async UniTask CastingPassiveSkillAsync(ICombatant attacker, ICombatant target)
        {
            if (_priorityICombatantList == null)
                return;
            
            foreach (var iCombatant in _priorityICombatantList)
            {
                if(iCombatant == null)
                    continue;
                
                // if (attacker != null)
                // {
                //     if (attacker.ETeam == iCombatant.ETeam)
                //         continue;
                // }
                    
                var passiveSkill = iCombatant.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Passive);
                if(passiveSkill == null)
                    continue;
                
                var targetList = GetTargetList(iCombatant, passiveSkill);
                var resTarget = target;
                // if (passiveSkill.)
                // {
                //     resTarget = targetList?.FirstOrDefault();
                // }
                
                MoveToTarget(iCombatant, passiveSkill, resTarget);
                iCombatant.IActCtr?.CastingSkill(this, passiveSkill, targetList); 
                    
                _sequenceActList?.Add(iCombatant.IActCtr);
            }

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        }

        private void MoveToTarget(ICombatant attacker, Skill skill, ICombatant target)
        {
            if (skill == null)
                return;
            
            var skillRange = skill.SkillData.Range;
            if (skillRange <= 0)
                return;

            if (target == null)
                return;
            
            var targetPos = target.Transform.position;
            var direction = targetPos.x - attacker.Transform.position.x;
            
            targetPos.x = direction < 0 ? targetPos.x + skillRange : targetPos.x - skillRange;
            targetPos.y -= 1f;
            targetPos.z = 0;

            SetSortingOrder(attacker, 1);
            
            attacker.IActCtr?.MoveToTarget(targetPos, reverse: targetPos.x - attacker.Transform.position.x < 0);
        }
        
        private void SetSortingOrder(ICombatant iCombatant, int sortingOrder)
        {
            var meshRenderer = iCombatant?.SkeletonAnimation?.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                return;
            
            meshRenderer.sortingOrder = sortingOrder;
        }
        
        private List<ICombatant> GetTargetList(ICombatant attacker, Skill skill)
        {
            if (skill == null)
                return null;
            
            List<ICombatant> iCombatantList = null;
            if (skill.SameTeam)
                iCombatantList =  attacker.ETeam == Type.ETeam.Ally ? _data?.AllyICombatantList : _data?.EnemyICombatantList;
            else
                iCombatantList =  attacker.ETeam == Type.ETeam.Ally ? _data?.EnemyICombatantList : _data?.AllyICombatantList;

            if (iCombatantList != null)
            {
                List<ICombatant> targetList = new();
                targetList.Clear();

                if (skill.ESkillTarget == Type.ESkillTarget.FarOne ||
                    skill.ESkillTarget == Type.ESkillTarget.NearOne)
                {
                    targetList.Add(iCombatantList.FirstOrDefault());

                    return targetList;
                }
                
                foreach (var targetIActor in iCombatantList)
                {
                    if(targetIActor == null)
                        continue;
                    
                    targetList.Add(targetIActor);
                }

                return targetList;
            }

            return null;
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

