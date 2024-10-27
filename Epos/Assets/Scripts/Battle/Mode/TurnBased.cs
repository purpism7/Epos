using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private Queue<IActor> _iActorQueue = null;
        private List<IActor> _priorityIActorList = null;
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

            _iActorQueue = new();
            _iActorQueue.Clear();
            
            switch (_data.EType)
            {
                case EType.ActionSpeed:
                {
                    _priorityIActorList = new();
                    _priorityIActorList.Clear();
                    
                    _priorityIActorList?.AddRange(_data.AllyIActorList);
                    _priorityIActorList?.AddRange(_data.EnemyIActorList);

                    // 임시로 initialize 진행. 차후 덱 편성 후 캐릭터 생성 시 진행 예정.
                    foreach (var iActor in _priorityIActorList)
                    {
                        (iActor as Character)?.Initialize();
                    }
                    
                    _priorityIActorList = _priorityIActorList?.OrderByDescending(iActor => iActor?.IStat?.Get(Stat.EType.ActionSpeed)).ToList();
     
                    break;
                }
            }
        }

        public override void Begin()
        {
            StartTurn();
        }
        
        private void StartTurn()
        {
            // var iActor = _iActorQueue?.Dequeue();
            // iActor.IActCtr.MoveToTarget(Vector3.one, () => { });

            SetTurn(_turn + 1);
            
            if (_priorityIActorList != null)
            {
                foreach (var iActor in _priorityIActorList)
                {
                    if(iActor == null)
                        continue;
                    
                    _iActorQueue?.Enqueue(iActor);
                }
            }
            
            
            
        }

        private void EndTurn()
        {
            
        }

        private void SetTurn(int turn)
        {
            _turn = turn;
            Debug.Log(turn);
            
        }
        
        private void Act()
        {
            var iActor = _iActorQueue?.Dequeue();
            if (iActor == null)
                return;

            var iActCtr = iActor.IActCtr;
            if (iActCtr == null)
                return;
            
            iActCtr.CastingSkill(this);
            


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

