using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

using Cysharp.Threading.Tasks;
using DG.Tweening;

public class Projectile : Common.Component<Projectile.Param>
{
    public class Param : Common.Param
    {
        public Transform TargetTm { get; private set; } = null;

        public Param WithTargetTm(Transform targetTm)
        {
            TargetTm = targetTm;
            return this;
        }
    }

    public Vector3 startPos;
    public Vector3 targetPos;

    readonly List<ParticleSystem.Particle> _enter = new();
    private ParticleSystem _particleSystem = null;

    public float startSpeed = 0f, endSpeed = 60f, accelTime = 2f;
    float currSpeed;
    Tween speedTween;

    private Vector3 _lastPos = Vector3.zero;


    public override UniTask InitializeAsync(Param param)
    {
        base.InitializeAsync(param);

        rootTm = GetComponent<Transform>();

        return UniTask.CompletedTask;
    }

    public override UniTask ActivateAsync(Param param)
    {
        base.ActivateAsync(param);

        return UniTask.CompletedTask;
    }

    private void LaunchTo()
    {

    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log(other);
    }

    private void OnParticleTrigger()
    {
        int entered = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enter);
        Debug.Log(entered);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        startPos.y += 2f;
        transform.position = startPos;

        speedTween = DOVirtual.Float(startSpeed, endSpeed, accelTime,
                                     v => currSpeed = v)
                               .SetEase(Ease.InCubic);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsActivate)
            return;

        var dir = targetPos - transform.position;

        //if (Physics.Raycast(_lastPos, dir.normalized, out RaycastHit hit, dir.magnitude))
        //{
        //    Debug.Log("hit");
        //    Extensions.SetActive(transform, false);
        //    return;
        //    // �浹 �������� ����Ʈ ����
        //    //Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        //    //Destroy(gameObject);
        //}

        transform.position = Vector3.MoveTowards(transform.position, targetPos, currSpeed * Time.deltaTime);

        var distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= 0.01f)
        {
            //Extensions.SetActive(transform, false);
            Deactivate();
        }

        _lastPos = transform.position;

    }
}
