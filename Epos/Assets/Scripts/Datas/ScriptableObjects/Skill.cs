using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;

namespace Datas.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Skill")]
    [System.Serializable]
    public class Skill : ScriptableObject
    {
        public int Id = 0;
        public ESkillCategory ESkillCategory = ESkillCategory.None;
        public float Cooltime = 0;
        public int Point = 0;
        
        public ESkillTarget ESkillTarget = ESkillTarget.None;
        public float Range  = 0;

        public bool SameTeam = false;

        public GameObject ProjectilePrefab = null;
    }
}

