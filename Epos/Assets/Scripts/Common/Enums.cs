using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public enum EClass
    {
        None,

        Knight,
        Archer,
        Assassin,
        Mechanic,
        Priest,
        Wizard,
    }
    
    public enum DirectionType
    {
        None,
        
        Forward,
        Back,
        Left,
        Right,
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

        Circle,
        Sector,
    }

    public enum EImpactType
    {
        None,

        Damage,
        Heal,
    }
    #endregion

    public enum EWeightType
    {
        None,

        Tactics,
        Mission,
    }

    public enum EmotionType
    {
        None,

        Agitation,
        Anger,
        Fatigue,
        Fear,
        Happy,
        Shout,
        Stun,
    }

    public enum  ProjectileEaseType
    {
        None,

        InCubic,
    }
}

    
   

