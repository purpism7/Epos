using System.Collections;
using System.Collections.Generic;
using Creature.Action;
using UnityEngine;

namespace Creature
{
    public class Monster : Character, IActor
    {
        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return "00_Idle";
                case Move: return "01_Run";
            }

            return string.Empty;
        }
    }
}
