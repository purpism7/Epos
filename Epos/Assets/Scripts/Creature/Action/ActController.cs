using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        
    }
    
    public class ActController : Controller, IActController
    {
        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            
            
            return this;
        }
    }
}

