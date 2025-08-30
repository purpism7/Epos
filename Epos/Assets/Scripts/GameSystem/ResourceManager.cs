using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace GameSystem
{
    public class ResourceManager : MonoBehaviour, IInitializable
    {
        [Inject] private AddressableManager _addressableManager = null;
        
        public AtlasLoader AtlasLoader { get; private set; } = null;

        void IInitializable.Initialize()
        {
            Debug.Log("ResourceManager Initialize");    
        }
        
        public async UniTask InitializeAsync()
        {
            AtlasLoader = new();
            await AtlasLoader.InitializeAsync(_addressableManager);
        }
    }
}

