using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public interface IHero
    {
        void MoveToTarget(Vector3 pos);
    }
    
    public class Hero : Character, IHero
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
        }
        
        #region IHero

        void IHero.MoveToTarget(Vector3 pos)
        {
            IActCtr?.MoveToTarget(pos);
        }
        #endregion
    }
}

