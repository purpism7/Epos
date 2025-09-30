using Common;
using Creator;
using Cysharp.Threading.Tasks;
using Datas.ScriptableObjects;
using DG.Tweening;
using GameSystem;
using GameSystem.Event;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using VContainer;

using Vector3 = UnityEngine.Vector3;

namespace Creature.Action
{
    public class Casting : Act<Casting.Param>
    {
        public class Param : ActParam
        {
            public IListener IListener = null;
            public ICombatant Attacker { get; private set; } = null;
            public Ability.ISkill ISkill = null;
            public ICombatant Target { get; private set; } = null;
            public List<ICombatant> TargetList { get; private set; } = null;

            public bool PlayAnimation = true;

            public Param WithAttacker(ICombatant attacker)
            {
                Attacker = attacker;
                return this;
            }

            public Param WithTarget(ICombatant target)
            {
                Target = target;
                return this;
            }
            public Param WithTargetList(List<ICombatant> targetList)
            {
                TargetList = targetList;
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

            var attacker = _param?.Attacker;
            if (attacker != null)
            {
                var direction = target.IActor.Transform.position - attacker.Transform.position;
                _param?.Attacker?.IActor?.IActCtr?.Flip(-direction.x);
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
                End();
                return;
            }

            if(skillData.PlayAnimation)
                await ActivateSpecialSkillAnimPopupAsync();

            PlayAnimation(skillData.AnimationName, false);
            
            var halfDuration = _duration / 2f;

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.InUse();
            ImpactToTargetList(skillData);

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            AfterCasting();
        }

        private void AfterCasting()
        {
            _param?.IListener?.AfterCasting(_param?.Attacker);
            _param?.ISkill?.EndCasting();
        }

        protected override void OnCompleted(TrackEntry trackEntry)
        {
            base.OnCompleted(trackEntry);

            End();
        }

        private void ImpactToTargetList(Skill skillData)
        {
            var attacker = _param?.Attacker;
            if (attacker == null)
                return;

            if (skillData.SameTeam)
            {
                foreach (var target in _param?.TargetList)
                {
                    if (target == null ||
                        !target.IActor.IsAlive)
                        continue;

                    var impactParam = new Impact.Param
                    {
                        PlayAnimation = _param.PlayAnimation,
                    }
                    .WithIStat(attacker.IStat)
                    .WithEImpactType(EImpactType.Heal);

                    target?.IActor?.IActCtr?.Impact(impactParam);
                }
            }
            else
            {
                // ���� ����
                if (skillData.ESkillTarget == ESkillTarget.Circle ||
                    skillData.ESkillTarget == ESkillTarget.Sector)
                    ImpactToMultipleTargetList(attacker, skillData);
                else
                    ImpactToSingleTarget(attacker, skillData);
            }
        }

        private void ImpactToMultipleTargetList(ICombatant attacker, Skill skillData)
        {
            if (attacker == null)
                return;

            var targetList = _param?.TargetList;
            if (targetList.IsNullOrEmpty())
                return;

            var closestTarget = attacker.Transform.FindClosestICombatant(targetList);

            foreach (var target in _param?.TargetList)
            {
                if (target == null ||
                    !target.IActor.IsAlive)
                    continue;

                bool isAttack = false;
                switch (skillData.ESkillTarget)
                {
                    case ESkillTarget.Circle:
                        {
                            isAttack = attacker.IsCircle(target);
                            Utils.DrawCircle(attacker.Transform.position, 360f, Color.black, 1f);
                            break;
                        }

                    case ESkillTarget.Sector:
                        {
                            isAttack = attacker.IsSector(target);
                            break;
                        }
                }

                if (isAttack)
                    ImpactToTarget(attacker, target, skillData);
            }
        }

        private void ImpactToSingleTarget(ICombatant attacker, Skill skillData)
        {
            if (attacker == null)
                return;

            var targetList = _param?.TargetList;
            if (targetList.IsNullOrEmpty())
                return;

            ICombatant resTarget = null;

            if (targetList.Count <= 1)
                resTarget = targetList.FirstOrDefault();
            else
            {
                switch(skillData.ESkillTarget)
                {
                    case ESkillTarget.NearOne:
                        {
                            resTarget = attacker.Transform.FindClosestICombatant(targetList);
                            break;
                        }

                    case ESkillTarget.FarOne:
                        {
                            resTarget = attacker?.FindFarthestICombatant(targetList);
                            break;
                        }
                }
            }

            if (resTarget != null)
            {
                if (skillData.HasProjectile)
                    CreateProjectile(skillData.ProjectilePrefab, resTarget);
                else
                    ImpactToTarget(attacker, resTarget, skillData);
            }
        }

        private void ImpactToTarget(ICombatant attacker, ICombatant target, Skill skillData)
        {
            if (attacker == null)
                return;

            if (target == null)
                return;

            var impactParam = new Impact.Param
            {
                PlayAnimation = _param.PlayAnimation,
            }
            .WithIStat(attacker.IStat)
            .WithEImpactType(EImpactType.Damage)
            .WithMultiplier(skillData.Multiplier);

            target?.IActor?.IActCtr?.Impact(impactParam);
            target?.HitAsync();

            if (skillData != null)
            {
                if (skillData.KnockbackDistance > 0)
                    KnockbackAsync(attacker, target, skillData.KnockbackDistance).Forget();
            }
        }

        private async UniTask KnockbackAsync(ICombatant attacker, ICombatant target, float knockbackDistance)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            //var attacker = _param?.Attacker;
            if (attacker == null)
                return;

            //var target = _param?.Target;
            if (target == null ||
               !target.IActor.IsAlive)
                return;

            var distance = knockbackDistance;
            var direction = (target.Transform.position - attacker.Transform.position).normalized;
            var targetPosition = target.Transform.position + direction * distance;

            // DoTween���� �̵�
            await target.Transform.DOMove(targetPosition, distance * 0.05f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => { });
        }

        private void CreateProjectile(GameObject proejctilePrefab, ICombatant targetICombatant)
        {
            var attacker = _param?.Attacker;
            if (attacker == null)
                return;

            var targetIActor = targetICombatant?.IActor;
            if (targetIActor == null)
                return;

            var projectileCreator = _iResolver?.Resolve<ProjectileCreator>();
            if (projectileCreator == null)
                return;

            var direction = targetIActor.Transform.position - attacker.Transform.position;
            float offsetX = 0;
            if (direction.x >= 0)
                offsetX = 2f;
            else
                offsetX  = -2f;

            var startPosition = attacker.Transform.position;
            startPosition.x += offsetX;

            var targetPoition = targetIActor.Transform.position;
            var endPosition = targetPoition + direction * 0.5f;
            endPosition.y += targetIActor.Height * 0.5f;

            var projectileParam = new Battle.Projectile.Param()
            {

            }
            .WithICaster(attacker)
            .WithTargetETeam(targetICombatant.ETeam)
            .WithStartPosition(startPosition)
            .WithEndPosition(endPosition);

            var iProjectile = projectileCreator?.Create(proejctilePrefab, projectileParam);
        }

        private async UniTask ActivateSpecialSkillAnimPopupAsync()
        {
            var uiCreator = _iResolver.Resolve <UIFactory>()?
                .Create<UI.Popup.SpecialSkillAnimPopup, UI.Popup.SpecialSkillAnimPopup.Param>();

            var specialSkillAnimPopup = await uiCreator
               .CreateAsync();
            specialSkillAnimPopup?.Activate();
        }
    }
}

