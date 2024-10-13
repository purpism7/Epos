using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity;

using Creature.Action;

namespace Creature
{
    public interface IActor : ISubject
    {
        Transform Transform { get; }
        SkeletonAnimation SkeletonAnimation { get; }
        IActController IActCtr { get; }
        IStat IStat { get; }

        string AnimationKey<T>(Act<T> act) where T : Act<T>.BaseData;
    }
}
