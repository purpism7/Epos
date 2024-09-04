using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace Creature
{
    public interface ISubject
    {
        
    }

    public interface IActor : ISubject
    {
        SkeletonAnimation SkeletonAnimation { get; }
    }
}

