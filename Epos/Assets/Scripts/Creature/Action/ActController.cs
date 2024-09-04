using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        void ChainUpdate();
    }
    
    public class ActController : Controller, IActController
    {
        private IActor _iActor = null;

        private Dictionary<System.Type, IAct> _iActDic = null;
        
        #region IActController
        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iActDic = new();
            _iActDic.Clear();
            
            return this;
        }

        void IActController.ChainUpdate()
        {
            
        }
        #endregion
    }
}

