using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class Skill
    {
        public Data.Skill SkillData { get; private set; } = null;

        public Type.ESkillCategory ESkillCategory { get; private set; } = Type.ESkillCategory.None;

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

        public void SetESkillCategory(Type.ESkillCategory eSkillCategory)
        {
            ESkillCategory = eSkillCategory;
        }
    }
}

