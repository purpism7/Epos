using Common;

using Cysharp.Threading.Tasks;
using Spine;

using Datas.ScriptableObjects;
using GameSystem.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vector3 = UnityEngine.Vector3;


namespace Creature.Action
{
    public class Casting : Act<Casting.Param>
    {
        public class Param : ActParam
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
            if (_param == null)
                return;

            // var eSkillCategory = _param.Skill.ESkillCategory;
            // _iActor?.IStat?.Add(eSkillCategory == ESkillCategory.Active ? Stat.EType.ActivePoint : Stat.EType.PassivePoint, -1f);

            LookAtTarget();
            CastingAsync().Forget();
        }

        private void LookAtTarget()
        {
            var target = _param.TargetList.FirstOrDefault();
            if (target == null)
                return;
            
            var direction = target.Transform.position - _param.ICombatant.Transform.position;
            if (direction.x > 0)
                _param.ICombatant.Transform.localScale = Vector3.one;
            else if (direction.x < 0)
                _param.ICombatant.Transform.localScale = new Vector3(-1, 1, 1);
        }

        private async UniTask CastingAsync()
        {
            _param?.IListener?.BeforeCasting();
          
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            SetAnimation(_param?.AnimationKey, false);
            
            var halfDuration = _duration / 2f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.InUse();
            
            if (_param?.TargetList != null &&
                !_param.Skill.SameTeam)
            {
                foreach (var target in _param.TargetList)
                {
                    target?.IActCtr?.TakeDamage(_param?.ICombatant, _param.PlayAnimation);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.AfterCasting(_param?.ICombatant);      
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);

            _endAction?.Invoke(_iActor);
        }

        // private ETeam ETeam
        // {
        //     get
        //     {
        //         ETeam eTeam = ETeam.None;
        //         var iCombatant = _param.ICombatant;
        //         if (iCombatant != null)
        //             eTeam = iCombatant.ETeam;
        //
        //         return eTeam;
        //     }
        // }
    }
}

