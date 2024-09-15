using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Move : Act<Move.Data>
    {
        public class Data : ActData
        {
            public string Key = string.Empty;
            public IListener IListener = null;
            public Vector3 TargetPos = Vector3.zero;
            public System.Action FinishAction = null;
        }
        
        public interface IListener
        {
            void Arrived();
        }
        
        public override void Execute(Data data)
        {
            base.Execute(data);
            
            Flip();
            
            // SetAnimation("01_F_Run", true);
            SetAnimation(data.Key, true);
        }

        private void Flip()
        {
            var iActor = _data?.IActor;
            if (iActor == null)
                return;
            
            var targetPos = _data.TargetPos;
            iActor.Transform.eulerAngles = new Vector3(0, iActor.Transform.position.x - targetPos.x < 0 ? 180f : 0, 0);
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            var iActor = _data?.IActor;
            if (iActor == null)
                return;

            var targetPos = _data.TargetPos;
            
            iActor.Transform.position = Vector3.MoveTowards(iActor.Transform.position, targetPos, Time.deltaTime * 10f);
            if (Vector3.Distance(iActor.Transform.position, targetPos) <= 0)
            {
                Debug.Log("Arrived");
                _data.IListener?.Arrived();
                _data.FinishAction?.Invoke();
            }
        }
    }
}
