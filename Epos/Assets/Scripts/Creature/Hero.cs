using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature.Action;
using Common;
using Creature.Emotion;

namespace Creature
{
    public class Hero : Character
    {
        public IEmotionController IEmotionCtr { get; private set; } = null;

        public override void Initialize()
        {
            base.Initialize();

            InitializeActController(this);
            // InitializeSkillController(this);
            InitializeEmotionController();

            CreateCombatant();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
        }

        private void InitializeEmotionController()
        {
            IEmotionCtr = transform.AddOrGetComponent<EmotionController>();
            _iResolver?.Inject(IEmotionCtr);

            IEmotionCtr?.Initialize(this);
        }

        #region Act
        public override string AnimationKey<T>(Act<T> act)
        {
            switch (act)
            {
                case Idle: return nameof(Idle);
                case Move move:
                    {
                        // if (move.IsJumpMove)
                        //     return "F_Jump";

                        return "Run";
                    }
                case Casting: return "Skill_01";
                case Damage: return nameof(Damage);
            }

            return string.Empty;
        }
        #endregion
    }
}

