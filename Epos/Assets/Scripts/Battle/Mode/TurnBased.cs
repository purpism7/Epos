using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Creature;

namespace Battle.Mode
{
    public class TurnBased : BattleMode<TurnBased.Data>
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

        private EType _eType = EType.None;
        private Queue<IActor> _iActorQueue = null;
        
        public override BattleMode<Data> Initialize(Data data)
        {
            base.Initialize(data);
            
            _iActorQueue = new();
            _iActorQueue.Clear();
            
            Initialize();
            
            return this;
        }

        private void Initialize()
        {
            if (_data == null)
                return;

            switch (_data.EType)
            {
                case EType.ActionSpeed:
                {
                    _data.AllyIActorList = _data.AllyIActorList
                        .OrderByDescending(iActor => iActor?.IStat?.Get(Stat.EType.ActionSpeed)).ToList();
                    
                    _data.EnemyIActorList = _data.EnemyIActorList
                        .OrderByDescending(iActor => iActor?.IStat?.Get(Stat.EType.ActionSpeed)).ToList();

                    break;
                }
            }
        }
    }
}

