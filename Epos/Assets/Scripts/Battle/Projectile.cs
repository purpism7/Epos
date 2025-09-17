using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

using VContainer;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Creature;

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

            public Vector3? StartPosition { get; private set; } = null;
            public Vector3? EndPosition { get; private set; } = null;

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
        }

        //readonly List<ParticleSystem.Particle> _enter = new();
        private ParticleSystem _particleSystem = null;

        private float startSpeed = 20f, endSpeed = 60f, accelTime = 3f;
        float currSpeed;
        Tween speedTween;

        private Vector3 _lastPos = Vector3.zero;

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

            speedTween = DOVirtual.Float(startSpeed, endSpeed, accelTime,
                                         v => currSpeed = v)
                                   .SetEase(Ease.InCubic);

            UpdateAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            Return();
        }

        //private void LaunchTo()
        //{

        //}

        //private void OnParticleCollision(GameObject other)
        //{
        //    Debug.Log(other);
        //}

        //private void OnParticleTrigger()
        //{
        //    int entered = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enter);
        //    Debug.Log(entered);
        //}

        //private void OnTriggerEnter2D(Collider2D other)
        //{
        //    Debug.Log(other);
        //}

        // Start is called once before the first execution of Update after the MonoBehaviour is created



        private async UniTask UpdateAsync()
        {
            var targetPosition = _param.EndPosition.Value;
            while (IsActivate)
            {
                var dir = targetPosition - transform.position;

                //if (Physics.Raycast(_lastPos, dir.normalized, out RaycastHit hit, dir.magnitude))
                //{
                //    Debug.Log("hit");
                //    Extensions.SetActive(transform, false);
                //    return;
                //    // �浹 �������� ����Ʈ ����
                //    //Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                //    //Destroy(gameObject);
                //}

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, currSpeed * Time.deltaTime);

                var distance = Vector3.Distance(transform.position, targetPosition);
                if (distance <= 0.01f)
                {
                    //Extensions.SetActive(transform, false);
                    Deactivate();
                }

                _lastPos = transform.position;

                await UniTask.Yield();
            }
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

