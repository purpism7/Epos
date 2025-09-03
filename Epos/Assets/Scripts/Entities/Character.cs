using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using Creator;
using Creature;
using Common;
using GameSystem;
using Lifetime;

namespace Entities
{
    public interface ICharacterManager : IManager
    {
        T Create<T>(int id, Transform rootTm) where T : Creature.Character;
    }

    public class Character : ICharacterManager
    {
        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private IObjectResolver _iResolver = null;

        private Dictionary<int, Creature.Character> _cachedDic = null;

        async UniTask IGeneric.InitializeAsync()
        {
            await UniTask.CompletedTask;
        }

        T ICharacterManager.Create<T>(int id, Transform rootTm)
        {
            if (_cachedDic == null)
            {
                _cachedDic = new();
                _cachedDic.Clear();
            }
            
            Creature.Character character = null;
            if (!_cachedDic.TryGetValue(id, out character))
            {
                GameObject loadGameObj = _addressableManager.LoadAssetByNameAsync<GameObject>(id.ToString());
                var gameObj = LifetimeScope.Instantiate(loadGameObj, rootTm);

                if (!gameObj)
                    return null;
                
                _iResolver?.InjectGameObject(gameObj);
                
                character = gameObj.GetComponent<T>();
                // var t = gameObj.GetComponent<T>();
                // Debug.Log(t);
                
                gameObj.SetActive(false);
                // character = new CharacterCreator<T>()
                //     .SetId(id)
                //     .SetRoot(rootTm)
                //     .Create;
            }

            var t = character as T;
            // var t = gameObj.GetComponent<T>();
            if (character == null)
                return null;
            
            character.SetActive(true);
            t?.Initialize();

            return t;
        }
    }
}

