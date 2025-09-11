using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using Datas.ScriptableObjects;
using GameSystem.Event;

namespace Ability
{
    public interface ISkill
    {
        Datas.ScriptableObjects.Skill SkillData { get; }

        bool IsReady { get; }
        float CooldownLeft { get; }

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
            Cooldown,
        }

        private float _currCooltime = 0f;
        private EState _eState = EState.None;

        public Datas.ScriptableObjects.Skill SkillData { get; private set; } = null;

        public bool IsReady { get { return _eState == EState.Ready; } }
        public float CooldownLeft { get { return _currCooltime; } }


        // Id만 넘기는 걸루 변경 예정. skill 데이터가 테이블 데이터로 변경 시.
        public virtual void Initialize(Datas.ScriptableObjects.Skill skillData)
        {
            SkillData = skillData;

            _eState = EState.Ready;
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
            _eState = EState.Cooldown;
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

