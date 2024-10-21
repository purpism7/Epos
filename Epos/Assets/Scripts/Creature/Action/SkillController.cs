using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface ISkillController : IController<ISkillController, ICaster>
    {
 
    }
    
    public class SkillController : Controller, ISkillController
    {
        private ICaster _iCaster = null;
        
        ISkillController IController<ISkillController, ICaster>.Initialize(ICaster iCaster)
        {
            _iCaster = iCaster;
            
            return this;
        }
        
        void IController<ISkillController, ICaster>.ChainUpdate()
        {
            
        }
    }
}

