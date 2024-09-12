using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

public interface ICharacterManager : IManager
{
    IHero FieldHero { get; }
}

public class CharacterManager : Manager, ICharacterManager
{
    // 임시
    [SerializeField] 
    private Hero fieldHero = null;
    
    private ICharacterGeneric _fieldICharacterGeneric = null;
    
    public override IManagerGeneric Initialize()
    {
        _fieldICharacterGeneric = fieldHero;
        _fieldICharacterGeneric?.Initialize();
        
        return this;
    }

    public override void ChainUpdate()
    {
        base.ChainUpdate();
        
        _fieldICharacterGeneric?.ChainUpdate();
    }
    
    #region ICharacterManager

    IHero ICharacterManager.FieldHero
    {
        get
        {
            return _fieldICharacterGeneric as IHero;
        }
    }
    #endregion
}
