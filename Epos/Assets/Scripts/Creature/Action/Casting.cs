using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine;

using Datas.ScriptableObjects;
using GameSystem.Event;
using Vector3 = UnityEngine.Vector3;


namespace Creature.Action
{
    public class Casting : Act<Casting.Param>
    {
        public class Param : ActParam
        {
            public IListener IListener = null;
            public ICaster ICaster { get; private set; } = null;
            public Ability.ISkill ISkill = null;
            public List<ICombatant> TargetList = null;
            public bool PlayAnimation = true;

            public Param WithICaster(ICaster iCaster)
            {
                ICaster = iCaster;
                return this;
            }
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

            LookAtTarget();
            CastingAsync().Forget();
        }

        private void LookAtTarget()
        {
            var target = _param.TargetList.FirstOrDefault();
            if (target == null)
                return;

            if(_param?.ICaster is ICombatant iCombatant)
            {
                var direction = target.IActor.Transform.position - iCombatant.Transform.position;
                iCombatant?.IActor?.IActCtr?.Flip(-direction.x);
            }
        }

        private async UniTask CastingAsync()
        {
            _param?.ISkill?.Casting();
            _param?.IListener?.BeforeCasting();
          
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            SetAnimation(_param?.AnimationKey, false);
            
            var halfDuration = _duration / 2f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.InUse();

            if (_param?.TargetList != null &&
                !_param.ISkill.SameTeam)
            {
                foreach (var target in _param.TargetList)
                {
                    if (target == null || 
                        !target.IActor.IsAlive)
                        continue;

                    var iCaster = _param?.ICaster;
                    // Temp
                    if (_param?.ISkill?.ProjectilePrefab != null)
                    {
                        var projectileGameObj = GameObject.Instantiate(_param.ISkill.ProjectilePrefab);
                        var projectile = projectileGameObj.GetComponent<Projectile>();
                        projectile.startPos = iCaster.Transform.position;
                        projectile.targetPos = target.IActor.Transform.position;

                        projectile?.InitializeAsync(null);
                        projectile?.ActivateAsync(null);
                    }

                    target?.IActor?.IActCtr?.TakeDamage(iCaster as ICombatant, _param.PlayAnimation);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.AfterCasting(_param?.ICaster as ICombatant);
            _param?.ISkill?.EndCasting();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);

            _endAction?.Invoke(_iActor);
        }
    }
}

