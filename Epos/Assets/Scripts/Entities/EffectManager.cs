using Cysharp.Threading.Tasks;
using UnityEngine;

using VContainer;
using VContainer.Unity;

using GameSystem;

namespace Entities
{
    public interface IEffectManager
    {
        Effect GetEffect(string key);
    }

    public class EffectManager : MonoBehaviour, IEffectManager
    {
        [Inject] private IObjectResolver _iResolver = null;
        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private ObjectPooler _objectPooler = null;

        Effect IEffectManager.GetEffect(string key)
        {
            Effect effect = _objectPooler?.Get<Effect>(key: key);
            if (effect == null)
            {
                var resKey = $"Assets/3_Resource/Effect/Prefabs/{key}.prefab";
                var prefab = _addressableManager?.LoadAssetByNameAsync<GameObject>(resKey);
                if(prefab)
                {
                    var gameObj = LifetimeScope.Instantiate(prefab, transform);
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

