using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity;

using Creature.Action;

namespace Creature
{
    public interface ISubject
    {
        
    }

    public interface IActor : ISubject
    {
        Transform Transform { get; }
        SkeletonAnimation SkeletonAnimation { get; }
        IActController IActCtr { get; }

        // 스탯으로 대체.
        float MoveSpeed { get; }
        string AnimationKey<T>(Act<T> act) where T : Act<T>.BaseData;
    }
}

