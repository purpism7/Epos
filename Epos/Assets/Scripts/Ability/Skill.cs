using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class Skill
    {
        public Datas.Skill SkillData { get; private set; } = null;

        public ESkillCategory ESkillCategory { get; private set; } = ESkillCategory.None;
        public bool SameTeam { get; private set; } = false;
        public ESkillTarget ESkillTarget { get; private set; } = ESkillTarget.None;

        public virtual void Initialize(Datas.Skill skillData)
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
        public void SetESkillCategory(ESkillCategory eSkillCategory)
        {
            ESkillCategory = eSkillCategory;
        }
        
        public void SetSameTeam(bool sameTeam)
        {
            SameTeam = sameTeam;
        }
        
        public void SetESkillTarget(ESkillTarget eSkillTarget)
        {
            ESkillTarget = eSkillTarget;
        }
        #endregion
    }
}

