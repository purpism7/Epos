using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public interface IParticipant
        {
            
        }

        private EType _eType = EType.None;
        private Queue<IParticipant> _iParticipantQueue = null;
        
        public override BattleMode<Data> Initialize(Data data)
        {
            // _eType = eType;
            
            _iParticipantQueue = new();
            _iParticipantQueue.Clear();

            return this;
        }
    }
}

