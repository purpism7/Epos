using Cinemachine;
using UnityEngine;

using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;

using Lifetime;
using GameSystem;
using Entities;
using Unity.VisualScripting;

namespace Scene
{
    public abstract class SceneInitializer : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera = null;
        [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
        
        protected LifetimeScope _lifetimeScope = null;

        private void Awake()
        {
            InitializeAsync().Forget();
        }
        
        private async UniTask InitializeAsync()
        {
            _lifetimeScope = FindFirstObjectByType<GlobalLifetimeScope>();
            if (_lifetimeScope == null)
            {
                GlobalLifetimeScope globalLifetimeScope = null;
                var obj = Resources.Load("GlobalLifetimeScope");
                if (obj != null)
                {
                    var gameObj = GameObject.Instantiate(obj);
                    if (gameObj)
                        globalLifetimeScope = gameObj.GetComponent<GlobalLifetimeScope>();
                }

                if (globalLifetimeScope != null)
                {
                    await globalLifetimeScope.InitializeAsync();
                    _lifetimeScope = globalLifetimeScope;
                }
            }
            
            _lifetimeScope = _lifetimeScope?.CreateChild(Configure);
            _lifetimeScope?.Container?.Inject(this);
            
            // await UniTask.
            // await UniTask.Yield();
            await OnInitializeAsync();

        }

        // public void CreateChild(LifetimeScope parentLifetimeScope)
        // {
        //     _lifetimeScope = parentLifetimeScope?.CreateChild(Configure);
        // }

        protected virtual async UniTask OnInitializeAsync()
        {
            var cameraManager = _lifetimeScope?.Container?.Resolve<ICameraManager>();
            if (cameraManager != null)
            {
                await cameraManager.InitializeAsync(mainCamera, virtualCamera);
                
                _lifetimeScope?.Container?.Resolve<UIManager>()?.StackUICamera(mainCamera);
            }
            
            var characterManager = _lifetimeScope?.Container?.Resolve<ICharacterManager>();
            if(characterManager != null)
                await characterManager.InitializeAsync();
            
            await UniTask.Yield();
        }

        protected virtual void Configure(IContainerBuilder builder)
        {
            Debug.Log("SceneInitializer.Configure");
            builder.RegisterEntryPoint<CharacterManager>(VContainer.Lifetime.Scoped).As<ICharacterManager>();
        }
    }
}
