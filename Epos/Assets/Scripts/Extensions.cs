using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine.Unity;
using Spine;
using System;

public static class Extensions
{
    public static T AddOrGetComponent<T>(this Component component) where T : Component
    {
        if (!component)
            return default;

        var t = component.GetComponent<T>();
        if (t == null)
        {
            t = component.gameObject.AddComponent<T>();
        }

        return t;
    }

    public static void SetActive(this Component component, bool active)
    {
        if (component == null)
            return;

        SetActiveAsync(component, active).Forget();
    }

    private static async UniTask SetActiveAsync(this Component component, bool active)
    {
        await UniTask.Yield();

        if (component == null)
            return;

        component.gameObject.SetActive(active);
    }

    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        if (list == null)
            return true;

        if (list.Count <= 0)
            return true;

        return false;
    }

    public static bool IsNullOrEmpty<T>(this T[] arrays)
    {
        if (arrays == null)
            return true;

        if (arrays.Length <= 0)
            return true;

        return false;
    }

    public static void RemoveAllChild(this Transform tm)
    {
        if (!tm)
            return;

        for (int i = tm.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(tm.GetChild(i)?.gameObject);
        }
    }

    public static void Initialize(this Transform tm)
    {
        if (!tm)
            return;

        tm.position = Vector3.zero;
        tm.rotation = Quaternion.identity;
        tm.localScale = Vector3.one;
    }

    // public static List<T> AddList<T, V>(this V[] arrays) where T : class
    // {
    //     if (arrays == null)
    //         return null;
    //         
    //     var list = new List<T>();
    //     list.Clear();
    //         
    //     foreach (V t in arrays)
    //     {
    //         if(t == null)
    //             continue;
    //             
    //         list.Add(t as T);
    //     }
    //
    //     return list;
    // }

    public static void PlayAnimation(this SkeletonAnimation skeletonAnimation, string animationName, bool loop, System.Action<TrackEntry> completedAction, out float duration)
    {
        duration = 0;

        try
        {
            var animationState = skeletonAnimation?.AnimationState;
            if (animationState == null)
                return;

            var animation = skeletonAnimation.skeletonDataAsset?.GetSkeletonData(true)?.Animations?
                .Find(animation => animation.Name.Contains(animationName));
            if (animation == null)
                return;

            var trackEntry = animationState.SetAnimation(0, animationName, loop);
            if (trackEntry == null)
                return;

            //skeletonAnimation.Update(Time.timeScale);

            trackEntry.Complete -= completedAction.Invoke;
            trackEntry.Complete += completedAction.Invoke;

            duration = trackEntry.Animation.Duration;
        }
        catch(Exception)
        {
            
        }
    }  
    
    public static void PlayAnimation(this SkeletonGraphic skeletonGraphic, string animationName, bool loop, System.Action<TrackEntry> completedAction, out float duration)
    {
        duration = 0;

        try
        {
            var animationState = skeletonGraphic?.AnimationState;
            if (animationState == null)
                return;
            
            var animation = skeletonGraphic.skeletonDataAsset?.GetSkeletonData(true)?.Animations?
                .Find(animation => animation.Name.Contains(animationName));
            if (animation == null)
                return;

            animationState.ClearTrack(0);
            var trackEntry = animationState.SetAnimation(0, animationName, loop);
            if (trackEntry == null)
                return;

            trackEntry.Complete -= completedAction.Invoke;
            trackEntry.Complete += completedAction.Invoke;

            duration = trackEntry.Animation.Duration;
        }
        catch(Exception)
        {
            
        }
    }  
}


