using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class Skill
    {
        public Datas.Skill SkillData { get; private set; } = null;

        public Type.ESkillCategory ESkillCategory { get; private set; } = Type.ESkillCategory.None;
        public bool SameTeam { get; private set; } = false;
        public Type.ESkillTarget ESkillTarget { get; private set; } = Type.ESkillTarget.None;

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
        public void SetESkillCategory(Type.ESkillCategory eSkillCategory)
        {
            ESkillCategory = eSkillCategory;
        }
        
        public void SetSameTeam(bool sameTeam)
        {
            SameTeam = sameTeam;
        }
        
        public void SetESkillTarget(Type.ESkillTarget eSkillTarget)
        {
            ESkillTarget = eSkillTarget;
        }
        #endregion
    }
}

