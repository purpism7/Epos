using UnityEngine;

using Spine.Unity;

using Characters;
using Characters.Actions;

namespace Actor
{
    public interface IActor
    {
        Transform Transform { get; }
        SkeletonAnimation  SkeletonAnimation { get; }
        IActionController ActionController { get; }
        CharacterAnimator CharacterAnimator { get; }
        
        void SetSortingOrder(float sortingOrder);
    }
}

