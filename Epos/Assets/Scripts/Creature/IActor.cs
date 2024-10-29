using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity;

using Creature.Action;

namespace Creature
{
    public interface IActor : ISubject
    {
        SkeletonAnimation SkeletonAnimation { get; }
        
        IStat IStat { get; }
        IActController IActCtr { get; }

        string AnimationKey<T>(Act<T> act) where T : Act<T>.BaseData;
    }
}
