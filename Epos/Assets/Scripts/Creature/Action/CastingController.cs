using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface ICastingController : IController<ICastingController, ICaster>
    {
 
    }
    
    public class CastingController : Controller, ICastingController
    {
        private ICaster _iCaster = null;
        
        ICastingController IController<ICastingController, ICaster>.Initialize(ICaster iCaster)
        {
            _iCaster = iCaster;
            
            return this;
        }
        
        void IController<ICastingController, ICaster>.ChainUpdate()
        {
            
        }
    }
}

