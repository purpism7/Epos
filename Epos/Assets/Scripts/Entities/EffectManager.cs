using Cysharp.Threading.Tasks;
using UnityEngine;

using VContainer;
using VContainer.Unity;

using GameSystem;

namespace Entities
{
    public interface IEffectManager
    {
        UniTask<Effect> GetEffectAsync(string key);
    }

    public class EffectManager : MonoBehaviour, IEffectManager
    {
        [Inject] private IObjectResolver _iResolver = null;
        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private ObjectPooler _objectPooler = null;

        async UniTask<Effect> IEffectManager.GetEffectAsync(string key)
        {
            Effect effect = _objectPooler?.Get<Effect>(key: key);
            if (effect == null)
            {
                var effectPath = $"Assets/3_Resource/Effect/Prefabs/{key}.prefab";
                var prefabGameObj = await _addressableManager.LoadAssetByNameAsync<GameObject>(effectPath);
                if(prefabGameObj)
                {
                    var gameObj = LifetimeScope.Instantiate(prefabGameObj, transform);
                    if (!gameObj)
                        return null;

                    _iResolver?.InjectGameObject(gameObj);

                    gameObj.transform.SetParent(transform);

                    effect = gameObj.GetComponent<Effect>();
                    effect?.Initialize();
                    
                    _objectPooler?.Add(effect);
                }
            }

            return effect;
        }
    }
}

