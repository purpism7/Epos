using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using Creature.Actions;
using Common;

namespace Creature
{
    public class Monster : Character, IActor
    {
        public override void Initialize()
        {
            base.Initialize();

            InitializeActController(this);
            InitializeEffectController(this);
        }
        
        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return nameof(Idle);

                case Move:
                case Trace:
                    return "Run";

                case Casting: return "Skill_01";

                case Impact:
                    return "Damege_01";

                case Die:
                    return "Damege";
            }

            return string.Empty;
        }

       
    }
}
