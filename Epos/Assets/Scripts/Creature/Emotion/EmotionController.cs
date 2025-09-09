using UnityEngine;

namespace Creature.Emotion
{
    public interface IEmotionController : IController<IEmotionController, IActor>
    {

    }

    public class EmotionController : Common.Component, IEmotionController
    {
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
    }
}

