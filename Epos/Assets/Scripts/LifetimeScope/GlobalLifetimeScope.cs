using VContainer;
using VContainer.Unity;

using GameSystem;
using UnityEngine;

namespace Lifetime
{
    public class GlobalLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIManager uiManager = null;
        
        protected override void Configure(IContainerBuilder builder)
        {

            // var gameObj = GameObject.Instantiate(uiManagerPrefab);
            // var lifetimeScope = gameObj.GetComponent<LifetimeScope>();
            
            builder.RegisterComponentInNewPrefab(uiManager, VContainer.Lifetime.Singleton)
                .UnderTransform(transform)
                .AsSelf();
        }
    }
}

