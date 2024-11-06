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

        // private EType _eType = EType.None;
        
        private List<ICombatant> _priorityICombatantList = null;
        private Queue<ICombatant> _iCombatantQueue = null;
        private Queue<IActController> _sequenceActQueue = null;
        private ICombatant _currICombatant = null;
        
        private int _turn = 0;
        
        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);
            
            _iCombatantQueue = new();
            _iCombatantQueue.Clear();

            _sequenceActQueue = new();
            
            return this;
        }

        /// <summary>
        /// 전투 시작.
        /// </summary>
        public override void Begin()
        {
            _iCombatantQueue?.Clear();
            
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
            _currICombatant?.IActCtr?.ChainUpdate();
        }
        
        /// <summary>
        /// 턴 시작.
        /// </summary>
        private async UniTask StartTurnAsync()
        {
            SetTurn(_turn + 1);
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            if (_priorityICombatantList != null)
            {
                foreach (var iCombatant in _priorityICombatantList)
                {
                    if(iCombatant == null)
                        continue;
                    
                    _iCombatantQueue?.Enqueue(iCombatant);
                }
            }
            
            ActAsync().Forget();
        }

        private void EndTurn()
        {
            
        }

        private void SetTurn(int turn)
        {
            _turn = turn;
            Debug.Log(turn);
        }
        
        private async UniTask ActAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            _sequenceActQueue?.Clear();
            _currICombatant = null;
            
            var iCombatant = _iCombatantQueue?.Dequeue();
            if (iCombatant == null)
                return;
            
            var activeSkill = iCombatant.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Active);
            if (activeSkill == null)
                return;
            
            var targetList = GetTargetList(iCombatant);
            var target = targetList?.FirstOrDefault();
            
            CastingPassiveSkill(iCombatant, target).Forget();
 
            MoveToTarget(iCombatant, activeSkill, target);
            iCombatant.IActCtr?.CastingSkill(this, activeSkill, targetList); 

            _currICombatant = iCombatant;
        }

        private async UniTask CastingPassiveSkill(ICombatant iCombatant, ICombatant target)
        {
            if (_priorityICombatantList != null)
            {
                foreach (var confrontICombatant in _priorityICombatantList)
                {
                    if(confrontICombatant == null)
                        continue;

                    if (iCombatant != null)
                    {
                        if (iCombatant.ETeam == confrontICombatant.ETeam)
                            continue;
                    }
                    
                    var passiveSkill = confrontICombatant.ISkillCtr?.GetPossibleSkill(Type.ESkillCategory.Passive);
                    if(passiveSkill == null)
                        continue;
                    
                    
                }
            }
        }

        private void MoveToTarget(ICombatant iCombatant, Skill skill, ICombatant target)
        {
            if (skill == null)
                return;
            
            var skillRange = skill.SkillData.Range;
            if (skillRange <= 0)
                return;

            if (target == null)
                return;
            
            var targetPos = target.Transform.position;
            targetPos.x += skillRange;
            targetPos.y -= 1f;
            targetPos.z = 0;
            
            iCombatant.IActCtr?.MoveToTarget(targetPos, () => { });
        }

        private List<ICombatant> GetTargetList(ICombatant iCombatant)
        {
            if (iCombatant == null)
                return null;
            
            List<ICombatant> iCombatantList = GetConfrontList(iCombatant); 
            if (iCombatantList != null)
            {
                List<ICombatant> targetList = new();
                targetList.Clear();
                
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

        private List<ICombatant> GetConfrontList(ICombatant iCombatant)
        {
            if (iCombatant is Hero)
                return _data?.EnemyICombatantList;
                
            return _data?.AllyICombatantList;
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

