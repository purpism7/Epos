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

    public static bool PlayAnimation(this SkeletonAnimation skeletonAnimation, string animationName, bool loop, System.Action<TrackEntry> completedAction, out float duration)
    {
        duration = 0;

        try
        {
            var animationState = skeletonAnimation?.AnimationState;
            if (animationState == null)
                return false;

            string resolvedAnimationName = ResolveAnimationName(animationState, animationName);
            if (string.IsNullOrEmpty(resolvedAnimationName))
                return false;

            animationState.ClearTracks();
            skeletonAnimation.skeleton?.SetToSetupPose();
            
            var trackEntry = animationState.SetAnimation(0, resolvedAnimationName, loop);
            if (trackEntry == null)
                return false;
            
            if (completedAction != null)
                trackEntry.Complete += completedAction.Invoke;

            duration = trackEntry.Animation.Duration;

            return true;
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }

        return false;
    }  
    
    public static void PlayAnimation(this SkeletonGraphic skeletonGraphic, string animationName, bool loop, System.Action<TrackEntry> completedAction, out float duration)
    {
        duration = 0;

        try
        {
            var animationState = skeletonGraphic?.AnimationState;
            if (animationState == null)
                return;
            
            string resolvedAnimationName = ResolveAnimationName(animationState, animationName);
            if (string.IsNullOrEmpty(resolvedAnimationName))
                return;

            animationState.ClearTracks();
            skeletonGraphic.Skeleton?.SetToSetupPose();

            var trackEntry = animationState.SetAnimation(0, resolvedAnimationName, loop);
            if (trackEntry == null)
                return;

            if (completedAction != null)
                trackEntry.Complete += completedAction.Invoke;

            duration = trackEntry.Animation.Duration;

            skeletonGraphic.Update(0);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }  

    private static string ResolveAnimationName(Spine.AnimationState animationState, string animationName)
    {
        if (animationState == null || string.IsNullOrEmpty(animationName))
            return string.Empty;

        var skeletonData = animationState.Data?.SkeletonData;
        if (skeletonData == null)
            return string.Empty;

        // Fast path: exact name lookup first.
        if (skeletonData.FindAnimation(animationName) != null)
            return animationName;

        var animations = skeletonData.Animations;
        if (animations == null)
            return string.Empty;

        for (int i = 0; i < animations.Count; i++)
        {
            var animation = animations.Items[i];
            if (animation == null)
                continue;

            if (animation.Name.Contains(animationName))
                return animation.Name;
        }

        return string.Empty;
    }
}


