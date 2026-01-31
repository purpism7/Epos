using Common;
using UnityEngine;

namespace Creature.Emotion
{
    public interface IEmotionController : IController<IEmotionController, IActor>
    {
        EmotionType EmotionType { get; }
        
        void UpdateEmotion(EmotionType emotionType);
    }

    public class EmotionController : Common.Component, IEmotionController
    {
        public EmotionType EmotionType { get; private set; } = EmotionType.None;

        #region IController
        IEmotionController IController<IEmotionController, IActor>.Initialize(IActor iActor)
        {
            // _iActor = iActor;


            return this;
        }

        void IController<IEmotionController, IActor>.ChainUpdate()
        {
            if (!IsActivate)
                return;
            
            // _currIAct?.ChainUpdate();
        }
        
        void IController<IEmotionController, IActor>.ChainFixedUpdate()
        {
            if (!IsActivate)
                return;
            
            // _currIAct?.ChainFixedUpdate();
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
    
        }
        #endregion
        
        #region IEmotionController

        void IEmotionController.UpdateEmotion(EmotionType emotionType)
        {
            EmotionType = emotionType;
        }
        #endregion
    }
}

