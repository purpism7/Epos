using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine;
using VContainer;

using Datas.ScriptableObjects;
using GameSystem.Event;
using Creator;

using Vector3 = UnityEngine.Vector3;
using GameSystem;

namespace Creature.Action
{
    public class Casting : Act<Casting.Param>
    {
        public class Param : ActParam
        {
            public IListener IListener = null;
            public ICombatant ICombatant { get; private set; } = null;
            public Ability.ISkill ISkill = null;
            public List<ICombatant> TargetList = null;
            public bool PlayAnimation = true;

            public Param WithICombatant(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
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

            var iCombatant = _param?.ICombatant;
            if (iCombatant != null)
            {
                var direction = target.IActor.Transform.position - iCombatant.Transform.position;
                _param?.ICombatant?.IActor?.IActCtr?.Flip(-direction.x);
            }
        }

        private async UniTask CastingAsync()
        {
            _param?.ISkill?.Casting();
            _param?.IListener?.BeforeCasting();
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            var skillData = _param?.ISkill?.SkillData;
            if (skillData == null)
            {
                _endAction?.Invoke(_iActor);
                return;
            }

            if(skillData.PlayAnimation)
                await ActivateSpecialSkillAnimPopupAsync();

            SetAnimation(skillData.AnimationName, false);
            
            var halfDuration = _duration / 2f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.InUse();
            DamageToTargetList(skillData);

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.AfterCasting(_param?.ICombatant);
            _param?.ISkill?.EndCasting();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);

            _endAction?.Invoke(_iActor);
        }

        private void DamageToTargetList(Skill skillData)
        {
            var iCombatant = _param?.ICombatant;
            if (_param?.TargetList != null &&
                !skillData.SameTeam)
            {
                foreach (var target in _param.TargetList)
                {
                    if (target == null || 
                        !target.IActor.IsAlive)
                        continue;

                    if (skillData.HasProjectile)
                        CreateProjectile(skillData.ProjectilePrefab, target);
                    else 
                        target?.IActor?.IActCtr?.TakeDamage(iCombatant, _param.PlayAnimation);
                }
            }
        }

        private void CreateProjectile(GameObject proejctilePrefab, ICombatant targetICombatant)
        {
            var iCombatant = _param?.ICombatant;
            if (iCombatant == null)
                return;

            if (targetICombatant == null)
                return;

            var projectileCreator = _iResolver?.Resolve<ProjectileCreator>();
            if (projectileCreator == null)
                return;

            var projectileParam = new Battle.Projectile.Param()
            {

            }
            .WithStartPosition(iCombatant.Transform.position)
            .WithEndPosition(targetICombatant.IActor.Transform.position);

            var iProjectile = projectileCreator?.Create(proejctilePrefab, projectileParam);
        }

        private async UniTask ActivateSpecialSkillAnimPopupAsync()
        {
            var uiCreator = _iResolver.Resolve <UIFactory>()?
                .Create<UI.Popup.SpecialSkillAnimPopup, UI.Popup.SpecialSkillAnimPopup.Param>(); ;
            //var rootRectTm = _uiManager?.CurrViewRectTm;
            //var uiCreator = _uiFactory?.Create<UI.Popup.BattleStart, UI.Popup.BattleStart.Param>();

            //var battleStartParam = new UI.Popup.BattleStart.Param()
            //    .WithCompletedAction(OnCompletedBattleStart);

            var specialSkillAnimPopup = await uiCreator
               //.SetRoot(rootRectTm)
               .CreateAsync();
            specialSkillAnimPopup?.Activate();
            //_battleStart?.ActivateAsync(battleStartParam);
        }
    }
}

