using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using Creature.Action;
using Common;
using Creature.Emotion;

namespace Creature
{
    public class Hero : Character, IEmotionalActor
    {
        public IEmotionController IEmotionCtr { get; private set; } = null;


        protected override void InitializeInject(IObjectResolver iResolver)
        {
            base.InitializeInject(iResolver);

            InitializeActController(this);
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeActController(this);
            InitializeEmotionController();
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
                case Impact impact: return "Damage";
            }

            return string.Empty;
        }
        #endregion
    }
}

