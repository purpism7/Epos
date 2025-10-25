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
                _param?.Attacker?.IActor?.IActCtr?.Flip(direction.x);
            }
        }

        //private async UniTask UpdateAsync()
        //{
        //    var attacker = _param?.Attacker;
        //    if (attacker == null)
        //        return;

        //    if (attacker.ETeam != ETeam.Ally)
        //        return;

        //    if (_param.ISkill.SkillData.ESkillTarget != ESkillTarget.Sector)
        //        return;

        //    while(_isUpdate)
        //    {
        //        //var Color = 
        //        Handles.color = new UnityEngine.Color(0, 1, 0, 0.3f);
        //        Vector2 startDirection = Quaternion.Euler(0, 0, 45f / 2f) * attacker.Transform.up;
        //        Handles.DrawSolidArc(attacker.Transform.position, Vector3.back, startDirection, 45f, 5f);

        //        //Vector3 boundary1 = Quaternion.Euler(0, 0, 45f / 2f) * attacker.Transform.right;
        //        //Vector3 boundary2 = Quaternion.Euler(0, 0, -45f / 2f) * attacker.Transform.right;

        //        //Gizmos.color = Color.red;
        //        //Debug.DrawRay(attacker.Transform.position, attacker.Transform.position + boundary1 * 5f, Color.yellow);
        //        //Debug.DrawRay(attacker.Transform.position, attacker.Transform.position + boundary2 * 5f, Color.yellow);

        //        //float angleThreshold = 30f;
        //        //Vector3 boundary1 = Quaternion.AngleAxis(angleThreshold, attacker.Transform.up) * attacker.Transform.right;
        //        //Vector3 boundary2 = Quaternion.AngleAxis(-angleThreshold, attacker.Transform.up) * attacker.Transform.right;
        //        //Debug.DrawRay(attacker.Transform.position, boundary1 * 10f, Color.yellow);
        //        //Debug.DrawRay(attacker.Transform.position, boundary2 * 10f, Color.yellow);

        //        await UniTask.Yield();
        //    }

            
        //}

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

            _iActor?.IEffectCtr?.Activate(skillData.AnimationName, new Effect.Param().WithTargetSkeletonAnimation(_iActor?.SkeletonAnimation), skillData.EffectName);

            var halfDuration = _duration / 2f;

            await UniTask.Delay(TimeSpan.FromSeconds(halfDuration));
            _param?.IListener?.InUse();
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

            // var closestTarget = attacker.Transform.FindClosestICombatant(targetList);

            foreach (var target in targetList)
            {
                if (target == null ||
                    !target.IActor.IsAlive)
                    continue;

                bool isAttack = false;
                switch (skillData.ESkillTarget)
                {
                    case ESkillTarget.Circle:
                        {
                            isAttack = attacker.IsCircle(target, 5f);
                            Utils.DrawCircle(attacker.Transform.position, 360f, UnityEngine.Color.black, 10f);
                            break;
                        }

                    case ESkillTarget.Sector:
                        {
                            isAttack = attacker.IsSector(target, 5f);
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

            //target.IActor.NavMeshAgent.

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

