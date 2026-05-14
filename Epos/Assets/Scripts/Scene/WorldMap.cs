using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

using GameSystem;
using Spine.Unity;
using VContainer;


namespace Scene
{
    public class WorldMap : SceneInitializer
    {
        [SerializeField] private Animator animator = null;
        [SerializeField] private SkeletonAnimation mapSkeletonAnimation = null;
        [SerializeField] private SkeletonAnimation cloudSkeletonAnimation = null;

        [Inject] private UIManager _uiManager = null;
        
        protected override async UniTask OnInitializeAsync()
        {
            await base.OnInitializeAsync();

            if (_uiManager != null)
                await _uiManager.FadeInOutAsync(() => UniTask.CompletedTask, OnCompleteFade);
        }

        private void OnCompleteFade()
        {
            cloudSkeletonAnimation?.PlayAnimation("Start_Cloud", false,
                (trackEntry) =>
                {
                    cloudSkeletonAnimation?.PlayAnimation("Idle_Cloud", true, null, out _);
                }, out _);
            
            mapSkeletonAnimation?.PlayAnimation("Start_Map", false,
                (trackEnty) =>
                {
                    mapSkeletonAnimation?.PlayAnimation("Idle_Map", true, null, out _);
                }, out _);
        }
    }
}
