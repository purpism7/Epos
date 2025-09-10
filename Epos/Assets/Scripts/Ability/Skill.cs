using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;
using Datas.ScriptableObjects;
using GameSystem.Event;
using Cysharp.Threading.Tasks;

namespace Ability
{
    public interface ISkill
    {
        ESkillTarget ESkillTarget { get; }
        float Range { get; }
        bool SameTeam { get; }

        GameObject ProjectilePrefab { get; }

        void Casting();
        void EndCasting();
    }

    public class Skill : ISkill
    {
        public enum EState
        {
            None,

            Ready,
            Casting,
            Cooltime,
        }

        private float _currCooltime = 0f;
        private EState _eState = EState.None;

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
            _eState = EState.Casting;
        }

        public virtual void EndCasting()
        {
            if (SkillData == null || SkillData.Cooltime <= 0f)
            {
                _eState = EState.Ready;
                return;
            }

            // 쿨타임 시작.
            UpdateCooltimeAsync().Forget();
        }

        private async UniTask UpdateCooltimeAsync()
        {
            _eState = EState.Cooltime;
            _currCooltime = SkillData.Cooltime;

            while (_currCooltime > 0f)
            {
                await UniTask.Yield();
                _currCooltime -= Time.deltaTime;
            }

            _eState = EState.Ready;
        }
    }
}

