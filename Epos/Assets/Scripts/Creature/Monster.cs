using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using Creature.Action;
using Common;

namespace Creature
{
    public class Monster : Character, IActor
    {
        protected override void InitializeInject(IObjectResolver iResolver)
        {
            base.InitializeInject(iResolver);

            InitializeActController(this);
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeEffectController(this);
        }

        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return nameof(Idle);
                case Move: return "Run";
                case Casting: return "Skill_01";
                case Impact: return "Damage";
                case Creature.Action.Die: return "Damage";
            }

            return string.Empty;
        }

       
    }
}
