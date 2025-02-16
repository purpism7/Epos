using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Datas.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Skill")]
    [System.Serializable]
    public class Skill : ScriptableObject
    {
        public int Id = 0;
        public Type.ESkillCategory ESkillCategory = Type.ESkillCategory.None;
        public int Point = 0;
        
        public Type.ESkillTarget ESkillTarget = Type.ESkillTarget.None;
        public int Range  = 0;
    }
}

