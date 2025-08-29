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

    public float startSpeed = 0f, endSpeed = 60f, accelTime = 1.5f;
    float currSpeed;
    Tween speedTween;


    public override UniTask InitializeAsync(Param param)
    {
        base.InitializeAsync(param);

        return UniTask.CompletedTask;
    }

    public override void Activate(Param param)
    {
        base.Activate(param);
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
        if (!transform.gameObject.activeSelf)
            return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, currSpeed * Time.deltaTime);

        var distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= 0.01f)
        {
            Extensions.SetActive(transform, false);
        }

    }
}
