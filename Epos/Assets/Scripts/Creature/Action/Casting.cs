using Common;
using Cysharp.Threading.Tasks;
using Datas.ScriptableObjects;
using GameSystem.Event;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Creature.Action
{
    public class Casting : Act<Casting.Data>
    {
        public class Data : BaseData
        {
            public IListener IListener = null;
            public ICombatant ICombatant = null;
            public Skill Skill = null;
            public List<ICombatant> TargetList = null;
            public bool PlayAnimation = true;
        }

        public interface IListener
        {
            void BeforeCasting();
            void InUse();
            void AfterCasting(ICombatant iCombatant);
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
            _iActor?.IStat?.Add(eSkillCategory == ESkillCategory.Active ? Stat.EType.ActivePoint : Stat.EType.PassivePoint, -1f);
            
            CastingAsync().Forget();
        }

        private async UniTask CastingAsync()
        {
            _data?.IListener?.BeforeCasting();
            // if (_data != null)
            //     GameSystem.Event.EventHandler.Notify(new SkillUseEventData(_data.Skill, ETeam));
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            SetAnimation(_data?.AnimationKey, false);


            // _data?.Skill.Casting();
            Debug.Log(_duration);
            var halfDuration = _duration / 2f;
            

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _data?.IListener?.InUse();
            
            if (_data?.TargetList != null &&
                !_data.Skill.SameTeam)
            {
                foreach (var target in _data.TargetList)
                {
                    target?.IActCtr?.TakeDamage(_data?.ICombatant, _data.PlayAnimation);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _data?.IListener?.AfterCasting(_data?.ICombatant);      
            
            //await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
           
            
            // if (_data != null)
            //     GameSystem.Event.EventHandler.Notify(new SkillUseEventData().WithETeam(ETeam));
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);

            _endAction?.Invoke();
        }

        // private ETeam ETeam
        // {
        //     get
        //     {
        //         ETeam eTeam = ETeam.None;
        //         var iCombatant = _data.ICombatant;
        //         if (iCombatant != null)
        //             eTeam = iCombatant.ETeam;
        //
        //         return eTeam;
        //     }
        // }
    }
}

