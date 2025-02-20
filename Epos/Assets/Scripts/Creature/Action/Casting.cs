using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Datas.ScriptableObjects;


namespace Creature.Action
{
    public class Casting : Act<Casting.Data>
    {
        public class Data : BaseData
        {
            public IListener IListener = null;
            public ICaster ICaster = null;
            public Skill Skill = null;
            public List<ICombatant> TargetList = null;
        }

        public interface IListener
        {
            void BeforeCasting();
            void InUse();
            void AfterCasting();
        }

        public override void Initialize(IActor iActor)
        {
            base.Initialize(iActor);
        }

        public override void Execute()
        {
            if (_data == null)
                return;

            var eSkillCategory = _data.Skill.ESkillCategory;
            _iActor?.IStat?.Add(eSkillCategory == Type.ESkillCategory.Active ? Stat.EType.ActivePoint : Stat.EType.PassivePoint, -1f);
            
            CastingAsync().Forget();
        }

        private async UniTask CastingAsync()
        {
            _data?.IListener?.BeforeCasting();
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            SetAnimation(_data?.AnimationKey, false);
            // _data?.Skill.Casting();

            var halfDuration = _duration / 2f;

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _data?.IListener?.InUse();
            
            if (_data?.TargetList != null &&
                !_data.Skill.SameTeam)
            {
                foreach (var target in _data?.TargetList)
                {
                    target?.IActCtr?.TakeDamage(_data?.ICaster);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _data?.IListener?.AfterCasting();            
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            _endAction?.Invoke();
        }
    }
}

