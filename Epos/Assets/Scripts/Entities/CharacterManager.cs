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
        UniTask<T> Create<T>(int id, Transform rootTm) where T : Creature.Character;
    }

    public class CharacterManager : ICharacterManager
    {
        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private IObjectResolver _iResolver = null;

        private Dictionary<int, Creature.Character> _cachedCharacters = null;

        async UniTask IGeneric.InitializeAsync()
        {
            await UniTask.CompletedTask;
        }

        async UniTask<T> ICharacterManager.Create<T>(int id, Transform rootTm)
        {
            if (_cachedCharacters == null)
                _cachedCharacters = new();

            Creature.Character character = null;
            if (!_cachedCharacters.TryGetValue(id, out character))
            {
                var addressableName = id.ToString();
                // await으로 교체
                GameObject loadGameObj = await _addressableManager.LoadAssetByNameAsync<GameObject>(addressableName);

                if (!loadGameObj)
                    return null;

                var gameObj = LifetimeScope.Instantiate(loadGameObj, rootTm);

                if (!gameObj)
                    return null;

                // Instantiate된 인스턴스가 어떤 addressable에서 왔는지 등록 →
                // 캐릭터가 더 이상 필요 없을 때 _addressableManager.ReleaseInstance(gameObj)로 정리 가능.
                _addressableManager?.TrackInstance(gameObj, addressableName);

                _iResolver?.InjectGameObject(gameObj);

                character = gameObj.GetComponent<T>();
                gameObj.SetActive(false);

                // 캐시에 추가 (기존 주석 해제)
                _cachedCharacters[id] = character;
            }

            if (character == null)
                return null;

            var t = character as T;
            if (t == null)
                return null;

            character.SetActive(true);
            t.Initialize();

            return t;
        }
    }
}

