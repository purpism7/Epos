using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEditor;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Spine;
using VContainer;

using GameSystem;
using GameSystem.Event;
using Datas.ScriptableObjects;
using Common;
using Creator;

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

        // 외부에서 스킬 정보 접근을 위한 프로퍼티
        public Ability.ISkill ISkill => _param?.ISkill;
        public Skill SkillData => _param?.ISkill?.SkillData;

        private bool _isUpdate = false;

        public override void Initialize(IActor iActor)
        {
            base.Initialize(iActor);
        }

        public override void Execute()
        {
            if (_param == null)
                return;

            _isUpdate = true;

            LookAtTarget();
            CastingAsync().Forget();
            //UpdateAsync().Forget();
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
                attacker.IActor?.IActCtr?.Flip(direction.x);
            }
        }

        private async UniTask CastingAsync()
        {
            var skillData = _param?.ISkill?.SkillData;
            if (skillData == null)
            {
                End();
                return;
            }

            _iActor?.IStat?.Add(Stat.EType.Mp, Stat.ESubType.None, -skillData.MP);

            _param?.IListener?.BeforeCasting();
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            _param?.ISkill?.Casting();

            if (skillData.PlayAnimation)
                await ActivateSpecialSkillAnimPopupAsync();

            PlayAnimation(skillData.AnimationName, false);

            _iActor?.IEffectCtr?.Activate(skillData.EffectName,new Effect.Param().WithTargetSkeletonAnimation(_iActor?.SkeletonAnimation), skillData.AnimationName);

            var halfDuration = _duration / 2f;

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.InUse();
            GameSystem.Event.EventHandler.Notify(new GameSystem.Event.SkillImpactEventData().WithISkill(_param?.ISkill));
            ImpactToTargetList(skillData);

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            AfterCasting();

            _iActor?.IEffectCtr?.Deactivate(skillData.AnimationName);

            _isUpdate = false;
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

            // 현재는 heal 만.
            if (skillData.SameTeam)
            {
                var targetList = _param?.TargetList;
                if (targetList != null)
                {
                    foreach (var target in targetList)
                    {
                        if (target == null ||
                            !target.IActor.IsAlive)
                            continue;

                        var impactParam = new Impact.Param
                        {
                            PlayAnimation = _param.PlayAnimation,
                        }
                        .WithIStat(attacker.IStat)
                        .WithImpactType(ImpactType.Heal);

                        target.IActor?.IActCtr?.Impact(impactParam);
                    }
                }
            }
            else
            {
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
            if (closestTarget == null)
                return;
            
            Vector2 direction = (closestTarget.Transform.position - attacker.Transform.position).normalized;
            float range = skillData.Range > 0f ? skillData.Range : 10f;
            const float sectorAngle = 60f;
            const float sectorRangeMultiplier = 2f;
            float sectorRange = range * sectorRangeMultiplier;

#if UNITY_EDITOR
            if (skillData.ESkillTarget == ESkillTarget.Sector)
                DrawSectorDebug(attacker.Transform.position, direction, sectorRange, sectorAngle, duration: 1.5f);
#endif

            foreach (var target in targetList)
            {
                if (target == null ||
                    !target.IActor.IsAlive)
                    continue;

                bool isAttack = false;
                switch (skillData.ESkillTarget)
                {
                    case ESkillTarget.Circle:
                        isAttack = attacker.IsCircle(target, range);
                        break;

                    case ESkillTarget.Sector:
                        isAttack = attacker.IsSector(target, direction, sectorRange, sectorAngle);
                        break;
                }

                if (isAttack)
                    ImpactToTarget(attacker, target, skillData);
            }

            // _isUpdate = false;
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
                            resTarget = attacker.FindFarthestICombatant(targetList);
                            break;
                        }
                }
            }

            if (resTarget != null)
            {
                if (skillData.HasProjectile)
                    CreateProjectileAsync(skillData, resTarget).Forget();
                else
                    ImpactToTarget(attacker, resTarget, skillData);
            }
        }

        private void ImpactToTarget(ICombatant attacker, ICombatant target, Skill skillData)
        {
            if (attacker == null)
                return;

            var targetActor = target?.IActor;
            if (targetActor == null)
                return;

            var impactParam = new Impact.Param
            {
                PlayAnimation = _param.PlayAnimation,
            }
            .WithIStat(attacker.IStat)
            .WithImpactType(GetImpactType(target.TeamType))
            .WithMultiplier(skillData.Multiplier);

            targetActor.IActCtr?.Impact(impactParam);
            target.HitAsync();
            
            targetActor.IEffectCtr?.Activate("Eff_Hit_01", new Effect.Param().WithTargetPosition(targetActor.Transform.position));

            if (skillData != null)
            {
                if (skillData.KnockbackDistance > 0)
                    KnockbackAsync(attacker, target, skillData.KnockbackDistance).Forget();
            }
        }

        private ImpactType GetImpactType(TeamType targetTeamType)
        {
            ImpactType impactType = ImpactType.None;
            if(targetTeamType == TeamType.Ally)
            {
                impactType = ImpactType.Damage;
            }
            else
            {
                impactType = ImpactType.PhysicalDamage;
            }

            return impactType;
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
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, distance, NavMesh.AllAreas))
                targetPosition = hit.position;
            
            // var targetPosition = target.Transform.position + direction * distance;

            await target.Transform.DOMove(targetPosition, distance * 0.05f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => { });
        }

        private async UniTask CreateProjectileAsync(Skill skillData, ICombatant targetICombatant)
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

            var startPosition = attacker.Transform.position + skillData.StartOffsetPosition;
            startPosition.x += offsetX;

            var targetPoition = targetIActor.Transform.position;
            var endPosition = targetPoition;

            if(skillData.Id != 10)
            {
                endPosition += direction * 0.5f;
                endPosition.y += targetIActor.Height * 0.5f;
            }

            for (int i = 0; i < skillData.BurstCount; ++i)
            {
                Vector2 resStartPosition = startPosition;
                if (skillData.Id == 10)
                    resStartPosition = new Vector2(startPosition.x, startPosition.y) + UnityEngine.Random.insideUnitCircle * skillData.spreadRadius;
                
                Vector2 resEndPosition = new Vector2(endPosition.x, endPosition.y) + UnityEngine.Random.insideUnitCircle * skillData.spreadRadius;
                float accelTime = skillData.AccelTime;
                if (skillData.spreadRadius > 0)
                    accelTime += UnityEngine.Random.Range(-1f, 1f);
                
                var projectileParam = new Battle.Projectile.Param()
                    .WithICaster(attacker)
                    .WithTargetTeamType(targetICombatant.TeamType)
                    .WithStartPosition(resStartPosition)
                    .WithEndPosition(resEndPosition)
                    .WithAccelTime(accelTime)
                    .WithDestroyOnHit(skillData.DestroyOnHit)
                    .WithHitEffectName(skillData.HitEffectName);

                projectileCreator.Create(skillData.ProjectilePrefab, projectileParam, Quaternion.Euler(0, -90f, 90f));
                
                await UniTask.Delay(TimeSpan.FromSeconds(skillData.BurstDelay));
            }
        }

        private async UniTask ActivateSpecialSkillAnimPopupAsync()
        {
            var uiCreator = _iResolver.Resolve<UIFactory>()?.Create<UI.Popup.SpecialSkillAnimPopup, UI.Popup.SpecialSkillAnimPopup.Param>(_iResolver);

            var specialSkillAnimPopup = await uiCreator
               .CreateAsync();
            specialSkillAnimPopup?.Activate();
        }

#if UNITY_EDITOR
        private static void DrawSectorDebug(Vector3 center, Vector2 direction, float range, float angle, float duration)
        {
            Vector2 dir = direction.normalized;
            float halfAngle = angle * 0.5f;
            UnityEngine.Color color = new UnityEngine.Color(1f, 0.5f, 0f, 0.9f);

            Vector2 rotMin = RotateDir(dir, -halfAngle);
            Vector2 rotMax = RotateDir(dir, halfAngle);

            Debug.DrawLine(center, center + (Vector3)(rotMin * range), color, duration);
            Debug.DrawLine(center, center + (Vector3)(rotMax * range), color, duration);

            const int segments = 24;
            Vector3 prev = center + (Vector3)(rotMin * range);
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float deg = -halfAngle + angle * t;
                Vector2 pt = RotateDir(dir, deg) * range;
                Vector3 next = center + (Vector3)pt;
                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
        }

        private static Vector2 RotateDir(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
#endif
    }
}

