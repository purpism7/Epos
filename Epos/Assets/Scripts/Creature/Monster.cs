using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Monster : Character
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
