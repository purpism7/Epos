using UnityEngine;

using Spine.Unity;

using Characters.Actions;

namespace Actor
{
    public interface IActor
    {
        Transform Transform { get; }
        SkeletonAnimation  SkeletonAnimation { get; }
        IActionController ActionController { get; }
        
        void SetSortingOrder(float sortingOrder);
    }
}

