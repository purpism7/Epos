using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

public interface ICharacterManager : IManager
{
   
}

public class CharacterManager : Manager, ICharacterManager
{
    public override IManagerGeneric Initialize()
    {
        return this;
    }
}
