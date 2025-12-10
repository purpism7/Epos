using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using VContainer;

using Common;
using Creature;
using Creature.Action;
using Entities;

namespace Battle
{
    public interface IProjectile
    {

    }

    public class Projectile : Common.Component<Projectile.Param>, IProjectile
    {
        public class Param : Common.Param
        {
            public ICaster ICaster { get; private set; } = null;
            //public Transform TargetTm { get; private set; } = null;

            public TeamType TargetTeamType { get; private set; } = TeamType.None;

            public Vector3? StartPosition { get; private set; } = null;
            public Vector3? EndPosition { get; private set; } = null;

            public float AccelTime { get; private set; } = 1f;
            public bool DestroyOnHit { get; private set; } = true;

            public string HitEffectName { get; private set; } = string.Empty;

            public Param WithICaster(ICaster iCaster)
            {
                ICaster = iCaster;
                return this;
            }

            public Param WithStartPosition(Vector3 startPosition)
            {
                StartPosition = startPosition;
                return this;
            }

            public Param WithEndPosition(Vector3 endPosition)
            {
                EndPosition = endPosition;
                return this;
            }

            public Param WithTargetTeamType(TeamType teamType)
            {
                TargetTeamType = teamType;
                return this;
            }
            
            public Param WithAccelTime(float accelTime)
            {
                AccelTime = accelTime;
                return this;
            }

            public Param WithDestroyOnHit(bool destroyOnHit)
            {
                DestroyOnHit = destroyOnHit;
                return this;
            }

            public Param WithHitEffectName(string hitEffectName)
            {
                HitEffectName = hitEffectName;
                return this;
            }
        }

        [Inject] private WeakTypeMap<IActor> _iActorMap = null;
        [Inject] private IEffectManager _effectManager = null;

        //readonly List<ParticleSystem.Particle> _enter = new();
        private ParticleSystem _particleSystem = null;

        private float startSpeed = 20f, endSpeed = 60f;
        float currSpeed;

        private Vector3 _lastPos = Vector3.zero;

        private void OnDrawGizmos()
        {
            //Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1);

            if(_param != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_param.EndPosition.Value, 1);
            }
        }

        [Inject]
        private void InitializeInject()
        {
            //Debug.Log("Projectile InitializeInject");
        }

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            _particleSystem = GetComponent<ParticleSystem>();
            rootTm = GetComponent<Transform>();

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            var startPosition = param.StartPosition.Value;
            startPosition.y += 2f;
            transform.position = startPosition;

            DOVirtual.Float(startSpeed, endSpeed, param.AccelTime, v => currSpeed = v)
                .SetEase(Ease.InCubic);

            UpdateAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            ActivateHitEffect();

            Return();
        }


        private async UniTask UpdateAsync()
        {
            var targetPosition = _param.EndPosition.Value;

            while (IsActivate)
            {
                if(_param.DestroyOnHit)
                {
                    var dir = targetPosition - transform.position;

                    var raycastHit = Physics2D.Raycast(_lastPos, dir.normalized, dir.magnitude);
                    if (raycastHit.collider != null)
                    {
                        var iActor = raycastHit.collider.transform.GetComponentInParent<IActor>();
                        if (iActor != null)
                        {
                            if (_iActorMap.TryGet<ICombatant>(iActor, out var iCombatant))
                            {
                                if (iCombatant.TeamType == _param.TargetTeamType &&
                                    iActor.IsAlive)
                                {
                                    var impactParam = new Impact.Param
                                    {
                                        PlayAnimation = false,
                                    }
                                    .WithIStat(_param?.ICaster?.IStat)
                                    .WithImpactType(ImpactType.Damage)
                                    ;

                                    iActor.IActCtr?.Impact(impactParam);
                                    iCombatant?.HitAsync();

                                    Deactivate();
                                    break;
                                }
                            }
                        }
                    }
                }

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, currSpeed * Time.deltaTime);

                var distance = Vector3.Distance(transform.position, targetPosition);
                if (distance <= 0.01f)
                {
                    Deactivate();
                    break;
                }

                _lastPos = transform.position;

                await UniTask.Yield();
            }
        }

        private void ActivateHitEffect()
        {
            var hitEffectName = _param?.HitEffectName;
            if (string.IsNullOrEmpty(hitEffectName))
                return;

            _effectManager?.GetEffect(hitEffectName)?
                .ActivateAsync(new Effect.Param().WithTargetPosition(transform.position));

            //_iActor?.IEffectCtr?.Activate(skillData.EffectName, new Effect.Param().WithTargetSkeletonAnimation(_iActor?.SkeletonAnimation), skillData.AnimationName);
        }
        // Update is called once per frame
        //void Update()
        //{
        //    if (!IsActivate)
        //        return;

        //    var dir = targetPos - transform.position;

        //    //if (Physics.Raycast(_lastPos, dir.normalized, out RaycastHit hit, dir.magnitude))
        //    //{
        //    //    Debug.Log("hit");
        //    //    Extensions.SetActive(transform, false);
        //    //    return;
        //    //    // �浹 �������� ����Ʈ ����
        //    //    //Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        //    //    //Destroy(gameObject);
        //    //}

        //    transform.position = Vector3.MoveTowards(transform.position, targetPos, currSpeed * Time.deltaTime);

        //    var distance = Vector3.Distance(transform.position, targetPos);
        //    if (distance <= 0.01f)
        //    {
        //        //Extensions.SetActive(transform, false);
        //        Deactivate();
        //    }

        //    _lastPos = transform.position;

        //}
    }
}

