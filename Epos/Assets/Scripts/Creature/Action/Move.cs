using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public class Move : Act
    {
        private IActor _iActor = null;
        private Vector3 _targetPos = Vector3.zero; 
        
        public override void Execute(IActor iActor)
        {
            _iActor = iActor;
            
            SetAnimation(iActor, "01_F_Run", true);

            _targetPos = new Vector3(120f, -320f);
            // Time.timeScale = 2f;
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            if (_iActor == null)
                return;
            
            _iActor.Transform.position = Vector3.MoveTowards(_iActor.Transform.position, _targetPos, Time.deltaTime * 10f);

            if (Vector3.Distance(_iActor.Transform.position, _targetPos) <= 0)
            {
                Debug.Log("Arrived");
            }
        }
    }
}
