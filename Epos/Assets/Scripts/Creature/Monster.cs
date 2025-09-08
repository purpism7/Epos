using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature.Action;
using Common;
using VContainer;

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

            InitializeCombatant(this);
        }

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

       
    }
}
