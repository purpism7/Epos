using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature.Action;
using Common;

namespace Creature
{
    public class Monster : Character
    {
        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return nameof(Idle);
                case Move: return "Run";
                case Casting: return "Skill_01";
                case Damage: return nameof(Damage);
                case Creature.Action.Die: return nameof(Damage);
            }

            return string.Empty;
        }
        
        public override void Initialize()
        {
            base.Initialize();

            IActCtr = transform.AddOrGetComponent<ActController>();
            IActCtr?.Initialize(this);

            // юс╫ц.
            if(Transform)
                Transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
}
