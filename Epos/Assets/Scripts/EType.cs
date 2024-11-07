using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Type
{
    public enum EFormation
    {
        None,
        
        Front,
        Rear,
    }
    
    public enum ETeam
    {
        None,
        
        Ally,
        Enemy,
    }
    
    #region Skill
    public enum ESkillCategory
    {
        None,
        
        Passive,
        Active,
    }

    public enum ESkillTarget
    {
        None,
        
        All,
        
        NearOne,
        FarOne,
    }
    #endregion
}
