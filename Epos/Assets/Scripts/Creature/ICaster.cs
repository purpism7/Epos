using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Spine.Unity;

namespace Creature
{
    public interface ICaster : ISubject
    {
        SkeletonAnimation SkeletonAnimation { get; }
        IStat IStat { get; }
    }
}
