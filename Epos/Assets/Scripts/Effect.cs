using System;
using UnityEngine;

using Cysharp.Threading.Tasks;


using Spine.Unity;

using Common;
using Creature;

public interface IEffect
{
    UniTask ActivateAsync(Effect.Param param);

    void Activate();
    void Deactivate();
    
    void ChainUpdate();
}

public class Effect : Component<Effect.Param>, IEffect
{
    public class Param : Common.Param
    {
        public Transform RootTm { get; private set; } = null;
        public SkeletonAnimation TargetSkeletonAnimation { get; private set; } = null;
        public Vector3? TargetPosition { get; private set; } = Vector3.zero;
        public bool ReturnParent { get; private set; } = false;

        public Param WithRootTm(Transform rootTm)
        {
            RootTm = rootTm;
            return this;
        }

        // public Param WithTargetTm(Transform targetTm)
        // {
        //     TargetTm = targetTm;
        //     return this;
        // }

        public Param WithTargetSkeletonAnimation(SkeletonAnimation targetSkeletonAnimation)
        {
            TargetSkeletonAnimation = targetSkeletonAnimation;
            return this;
        }

        public Param WithTargetPosition(Vector3? targetPosition)
        {
            TargetPosition = targetPosition;
            return this;
        }

        public Param WithReturnParent(bool returnParent)
        {
            ReturnParent = returnParent;
            return this;
        }
    }

    [SerializeField] private new ParticleSystem particleSystem = null;

    // private ParticleSystem _particleSystem = null;
    private float _direction = 0f;
    private float _lifetime = 0f;
    
    public override void Initialize()
    {
        base.Initialize();

        // particleSystem = GetComponentInChildren<ParticleSystem>();
        if(particleSystem != null)
        {
            var main = particleSystem.main;
            _lifetime = main.startDelay.constantMax + main.duration + main.startLifetime.constantMax;
            //Debug.Log(_lifetime);
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

        if (param.TargetPosition != null)
            transform.localPosition = param.TargetPosition.Value;

        transform.localRotation = Quaternion.identity;

        // UpdateDirection();

        //Debug.Log(_lifetime);
        if (particleSystem != null)
        {
            particleSystem.Play();
 
            if (_lifetime > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_lifetime));

                Deactivate();
            }
        }
    }

    public override void Activate()
    {
        base.Activate();
        
        Extensions.SetActive(transform, true);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        
        Return(_param.ReturnParent);
    }

    private void UpdateDirection()
    {
        // var targetTm = _param?.TargetTm;
        // if (targetTm)
        // {
        //     // if (Mathf.Abs(_direction - targetTm.localScale.x) > float.Epsilon)
        //     // if (_direction != targetTm.localScale.x)
        //     if (!Mathf.Approximately(_direction, targetTm.localScale.x))
        //     {
        //         transform.localScale = new Vector3(targetTm.localScale.x, 1f, 1f);
        //         _direction = targetTm.localScale.x;
        //     }
        //
        //     return;
        // }

        var targetSkeletonAnimation = _param?.TargetSkeletonAnimation;
        if (targetSkeletonAnimation != null)
        {
            var skeleton = targetSkeletonAnimation.Skeleton;
            
            // float.Epsilon (아주 작은 값)을 사용하여 안전하게 비교.
            // 두 값의 차이가 허용 오차보다 크다면 -> 방향이 바뀐 것으로 간주.
            if (!Mathf.Approximately(_direction, skeleton.ScaleX))
            // if (Mathf.Abs(_direction - skeleton.ScaleX) > float.Epsilon)
            // if (_direction != skeleton.ScaleX)
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
