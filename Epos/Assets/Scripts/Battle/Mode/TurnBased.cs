using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;

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
        private Queue<ICombatant> _iCombatantQueue = null;
        private List<ICombatant> _priorityICombatantList = null;
        private ICombatant _currICombatant = null;
        private int _turn = 0;
        
        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);
            
            Initialize();
            
            return this;
        }

        private void Initialize()
        {
            if (_data == null)
                return;

            _iCombatantQueue = new();
            _iCombatantQueue.Clear();
            
            switch (_data.EType)
            {
                case EType.ActionSpeed:
                {
                    _priorityICombatantList = new();
                    _priorityICombatantList.Clear();
                    
                    _priorityICombatantList?.AddRange(_data.AllyICombatantList);
                    _priorityICombatantList?.AddRange(_data.EnemyICombatantList);

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
        }

        public override void Begin()
        {
            StartTurn();
        }

        public override void ChainUpdate()
        {
            _currICombatant?.IActCtr?.ChainUpdate();
        }
        
        private void StartTurn()
        {
            // var iActor = _iActorQueue?.Dequeue();
            // iActor.IActCtr.MoveToTarget(Vector3.one, () => { });

            SetTurn(_turn + 1);
            
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
            var iCombatant = _iCombatantQueue?.Dequeue();
            if (iCombatant == null)
                return;

            // var iActCtr = iActor.IActCtr;
            // if (iActCtr == null)
            //     return;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            var targetList = GetTargetList(iCombatant);
            var skill = iCombatant.ISkillCtr?.PossibleActiveSkill;
            
            var target = targetList?.FirstOrDefault();
            if (target != null)
            {
                var targetPos = target.Transform.position;
                targetPos.x += skill.SkillData.Range;
                targetPos.y -= 1f;
                targetPos.z = 0;
            
                iCombatant.IActCtr?.MoveToTarget(targetPos, () => { });
            }
            
            iCombatant.IActCtr?.CastingSkill(this, skill, targetList);

            _currICombatant = iCombatant;
        }

        private List<ICombatant> GetTargetList(ICombatant iCombatant)
        {
            if (iCombatant == null)
                return null;
            
            List<ICombatant> iCombatantList = null; 
            
            if (iCombatant is Hero)
            {
                iCombatantList = _data?.EnemyICombatantList;
            }
            else if (iCombatant is Monster)
            {
                iCombatantList = _data?.AllyICombatantList;
            }

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

