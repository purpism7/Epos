using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Ability;


namespace Creature.Action
{
    public class Casting : Act<Casting.Data>
    {
        public class Data : BaseData
        {
            public IListener IListener = null;
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
            
            CastingAsync().Forget();
        }

        private async UniTask CastingAsync()
        {
            _data?.IListener?.BeforeCasting();

            await UniTask.Yield();
            
            SetAnimation(_data.AnimationKey, false);
            _data.Skill.Casting();

            await UniTask.Delay(TimeSpan.FromSeconds(_duration));
            
            _endAction?.Invoke();
        }
    }
}

