using UnityEngine;

using Cysharp.Threading.Tasks;


using Spine.Unity;

using Common;
using Creature;

public interface IEffect
{
    UniTask ActivateAsync(Effect.Param param);
    void Deactivate();
    void ChainUpdate();
}

public class Effect : Component<Effect.Param>, IEffect
{
    public class Param : Common.Param
    {
        public Transform RootTm { get; private set; } = null;

        public Transform TargetTm { get; private set; } = null;
        public SkeletonAnimation TargetSkeletonAnimation { get; private set; } = null;

        public Param WithRootTm(Transform rootTm)
        {
            RootTm = rootTm;
            return this;
        }

        public Param WithTargetTm(Transform targetTm)
        {
            TargetTm = targetTm;
            return this;
        }

        public Param WithTargetSkeletonAnimation(SkeletonAnimation targetSkeletonAnimation)
        {
            TargetSkeletonAnimation = targetSkeletonAnimation;
            return this;
        }
    }

    private ParticleSystem _particleSystem = null;
    private float _direction = 0f;
    private float _lifetime = 0f;

    public override void Initialize()
    {
        base.Initialize();

        _particleSystem = GetComponent<ParticleSystem>();
        if(_particleSystem != null)
        {
            var main = _particleSystem.main;
            _lifetime = main.startDelay.constantMax + main.duration + main.startLifetime.constantMax;
        }  
    }

    public override async UniTask ActivateAsync(Param param)
    {
        await base.ActivateAsync(param);

        if (param?.RootTm)
        { 
            if(transform.parent != param.RootTm)
                transform.SetParent(param.RootTm);
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        UpdateDirection();

        //Debug.Log(_lifetime);
    }

    private void UpdateDirection()
    {
        var targetTm = _param?.TargetTm;
        if (targetTm)
        {
            if (_direction != targetTm.localScale.x)
            {
                transform.localScale = new Vector3(targetTm.localScale.x, 1f, 1f);
                _direction = targetTm.localScale.x;
            }

            return;
        }

        var targetSkeletonAnimation = _param?.TargetSkeletonAnimation;
        if (targetSkeletonAnimation != null)
        {
            var skeleton = targetSkeletonAnimation.Skeleton;
            if (_direction != skeleton.ScaleX)
            {
                transform.localScale = new Vector3(skeleton.ScaleX, 1f, 1f);
                _direction = skeleton.ScaleX;
            }

            return;
        }
    }

    void IEffect.ChainUpdate()
    {
        if (!IsActivate)
            return;

        UpdateDirection();
    }
}
