using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class Skill
    {
        public Data.Skill SkillData { get; private set; } = null;

        public Type.ESkillCategory ESkillCategory { get; private set; } = Type.ESkillCategory.None;
        public Type.ETeam ETargetTeam { get; private set; } = Type.ETeam.None;
        public Type.ESkillTarget ESkillTarget { get; private set; } = Type.ESkillTarget.None;

        public virtual void Initialize(Data.Skill skillData)
        {
            SkillData = skillData;
        }

        public virtual void ChainUpdate()
        {
            
        }

        public virtual void Casting()
        {
            
        }

        #region 데이터화 예정
        public void SetESkillCategory(Type.ESkillCategory eSkillCategory)
        {
            ESkillCategory = eSkillCategory;
        }
        
        public void SetETargetTeam(Type.ETeam eTeam)
        {
            ETargetTeam = eTeam;
        }
        
        public void SetESkillTarget(Type.ESkillTarget eSkillTarget)
        {
            ESkillTarget = eSkillTarget;
        }
        #endregion
    }
}

