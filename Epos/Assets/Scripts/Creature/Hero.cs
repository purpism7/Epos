using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using VContainer;

using Common;
using Creature.Action;
using Creature.Emotion;
using UnityEditor;

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

            InitializeEmotionController();
            InitializeEffectController(this);
        }

        public override void Deactivate()
        {
            _iStatGeneric?.Deactivate();
            IActCtr?.Deactivate();
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
                case Trace:
                    {
                        // if (move.IsJumpMove)
                        //     return "F_Jump";

                        return "Run";
                    }
                case Casting: return "Skill_01";
                case Impact:
                case Die:
                    return "Damege";
            }

            return string.Empty;
        }
        #endregion
    }
}

