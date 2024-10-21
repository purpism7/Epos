using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Move : Act<Move.Data>
    {
        public class Data : BaseData
        {
            // public IListener IListener = null;
            public Vector3 TargetPos = Vector3.zero;
            public System.Action FinishAction = null;
        }
        
        // public interface IListener
        // {
        //     void Arrived();
        // }

        public override void Execute()
        {
            if (_data == null)
                return;
            
            Flip();
            
            SetAnimation(_data.AnimationKey, true);
        }

        private void Flip()
        {
            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;
            
            var targetPos = _data.TargetPos;
            iActorTm.eulerAngles = new Vector3(0, iActorTm.position.x - targetPos.x < 0 ? 0 : 180f, 0);
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;
            
            if (_iActor?.IStat == null)
                return;

            var targetPos = _data.TargetPos;
            var moveSpeed = _iActor.IStat.Get(Stat.EType.MoveSpeed);
            
            iActorTm.position = Vector3.MoveTowards(iActorTm.position, targetPos, Time.deltaTime * moveSpeed);
            if (Vector3.Distance(iActorTm.position, targetPos) <= 0)
            {
                // _data.IListener?.Arrived();
                _data.FinishAction?.Invoke();
                
                _endAction?.Invoke();
            }
        }
    }
}
