using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace GameSystem
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public AtlasLoader AtlasLoader { get; private set; }

        protected override void Initialize()
        {
            
        }

        public async UniTask InitializeAsync()
        {
            AtlasLoader = new();
            await AtlasLoader.InitializeAsync();
        }
    }
}

