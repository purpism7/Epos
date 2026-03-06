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
        public string AnimationName = string.Empty;
        public string EffectName = string.Empty;
        public ESkillCategory ESkillCategory = ESkillCategory.None;
        public float Cooltime = 0;
        public float MP = 0;
        public int Point = 0;

        public ESkillTarget ESkillTarget = ESkillTarget.None;
        public float Range  = 0;
        public float Multiplier = 1f;

        public bool SameTeam = false;

        public float KnockbackDistance = 0;
        public string DashAnimationName = string.Empty;

        public bool ShakeCamera = false;

        // TODO: Projectile Table 
        [Header("Projectile")]
        public GameObject ProjectilePrefab = null;
        public int BurstCount = 1;
        public float BurstDelay = 0;
        public bool PlayAnimation = false;
        public Vector3 StartOffsetPosition = Vector3.zero;
        public float AccelTime = 1f;
        public bool DestroyOnHit = true;
        public string HitEffectName = string.Empty;
        public int spreadRadius = 0;

        public bool HasProjectile { get { return ProjectilePrefab != null; } }
    }
}

