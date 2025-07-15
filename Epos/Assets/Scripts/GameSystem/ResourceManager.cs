using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace GameSystem
{
    public class ResourceManager : Singleton<ResourceManager>, IInitializable
    {
        public AtlasLoader AtlasLoader { get; private set; } = null;

        protected override void Initialize()
        {
            //DontDestroyOnLoad(this);
        }

        void IInitializable.Initialize()
        {
            Debug.Log("ResourceManager Initialize");    
        }
        public async UniTask InitializeAsync()
        {
            AtlasLoader = new();
            await AtlasLoader.InitializeAsync();
        }
    }
}

