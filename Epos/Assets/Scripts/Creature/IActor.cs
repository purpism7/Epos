using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine.Unity;

using Creature.Action;
using Creature.Emotion;

namespace Creature
{
    public interface IActor : ISubject
    {
        SkeletonAnimation SkeletonAnimation { get; }
        Collider2D Collider { get; }

        IStat IStat { get; }
        IActController IActCtr { get; }
        ICreatureEffectController IEffectCtr { get; }

        string AnimationKey<T>(Act<T> act) where T : ActParam;

        void SortingOrder(float order);
    }

    public interface IEmotionalActor : IActor
    {
        IEmotionController IEmotionCtr { get; }
    }
}
