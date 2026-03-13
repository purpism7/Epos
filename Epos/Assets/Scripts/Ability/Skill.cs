using Common;
using Cysharp.Threading.Tasks;
using Datas.ScriptableObjects;
using GameSystem.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
        private CancellationTokenSource _cts = null;

        public Datas.ScriptableObjects.Skill SkillData { get; private set; } = null;

        public bool IsReady { get { return _eState == EState.Ready; } }
        public float CooldownLeft { get { return _currCooltime; } }

#if UNITY_EDITOR
        public EState State { get { return _eState; } }
#endif


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
            if (_eState == EState.Cooldown) 
                return;

            if (SkillData == null || SkillData.Cooltime <= 0f)
            {
                _eState = EState.Ready;
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            UpdateCooltimeAsync(_cts.Token).Forget();
        }

        private async UniTask UpdateCooltimeAsync(CancellationToken token)
        {
            _eState = EState.Cooldown;
            _currCooltime = SkillData.Cooltime;

            try
            {
                while (_currCooltime > 0f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);

                    _currCooltime -= Time.deltaTime;
                }

                _eState = EState.Ready;
            }
            catch (OperationCanceledException)
            {

            }
        }
    }
}

