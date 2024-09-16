using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public interface IMonster
    {
        
    }
    
    public class Monster : Character, IMonster
    {
        public override float MoveSpeed
        {
            get
            {
                return 2f;
            }
        }
    }
}
