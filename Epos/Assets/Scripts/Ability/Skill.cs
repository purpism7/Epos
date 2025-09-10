using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;
using Datas.ScriptableObjects;
using GameSystem.Event;

namespace Ability
{
    public interface ISkill
    {
        ESkillTarget ESkillTarget { get; }
        float Range { get; }
        bool SameTeam { get; }

        GameObject ProjectilePrefab { get; }
    }

    public class Skill : ISkill
    {
        public Datas.ScriptableObjects.Skill SkillData { get; private set; } = null;

        //public ESkillCategory ESkillCategory { get; private set; } = ESkillCategory.None;
        //public bool SameTeam { get; private set; } = false;
        //public ESkillTarget ESkillTarget { get; private set; } = ESkillTarget.None;
        public ESkillTarget ESkillTarget { get { return SkillData != null ? SkillData.ESkillTarget : ESkillTarget.None; } }
        public float Range { get { return SkillData != null ? SkillData.Range : 0f; } }
        public bool SameTeam { get { return SkillData != null ? SkillData.SameTeam : false; } }
        public GameObject ProjectilePrefab { get { return SkillData != null ? SkillData.ProjectilePrefab : null; } }

        // Id만 넘기는 걸루 변경 예정. skill 데이터가 테이블 데이터로 변경 시.
        public virtual void Initialize(Datas.ScriptableObjects.Skill skillData)
        {
            SkillData = skillData;
        }

        public virtual void ChainUpdate()
        {
            
        }

        public virtual void Casting()
        {
            
        }
    }
}

