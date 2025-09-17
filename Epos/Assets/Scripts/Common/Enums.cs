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

    public enum EEmotionType
    {
        None,

        Encourage,
        CalmDown,
        Focus,
        FireUp,
        Relax,
        DemandMore,
        Praise,
        Berate,
    }
}

    
   

