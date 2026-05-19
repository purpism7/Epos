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
        float CooldownTotal { get; }

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
        private float _cooltime = 0f;
        private EState _eState = EState.None;
        private CancellationTokenSource _cancellationTokenSource = null;

        public Datas.ScriptableObjects.Skill SkillData { get; private set; } = null;

        public bool IsReady => _eState == EState.Ready;
        public float CooldownLeft => _currCooltime;
        public float CooldownTotal => _cooltime;

#if UNITY_EDITOR
        public EState State => _eState;
#endif

        public virtual void Initialize(Datas.ScriptableObjects.Skill skillData)
        {
            SkillData = skillData;

            _eState = EState.Ready;

            if (SkillData != null && SkillData.InitialCooltime > 0f)
                StartCooltime(SkillData.InitialCooltime);
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

            StartCooltime(SkillData.Cooltime);
        }

        private void StartCooltime(float cooltime)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            UpdateCooltimeAsync(cooltime, _cancellationTokenSource.Token).Forget();
        }

        private async UniTask UpdateCooltimeAsync(float cooltime, CancellationToken cancellationToken)
        {
            _eState = EState.Cooldown;
            _currCooltime = cooltime;
            _cooltime = cooltime;

            try
            {
                while (_currCooltime > 0f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                    _currCooltime -= Time.deltaTime;
                }

                // _eState = EState.Ready;
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _currCooltime = 0f;
                    _eState = EState.Ready;
                }
            }
        }
    }
}
