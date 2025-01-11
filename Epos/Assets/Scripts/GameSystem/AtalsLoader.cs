using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

using Cysharp.Threading.Tasks;

namespace GameSystem
{
    public class AtlasLoader
    {
        private Dictionary<string, SpriteAtlas> _spriteAtlasDic = new();
        
        public async UniTask InitializeAsync()
        {
            _spriteAtlasDic?.Clear();
            
            await AddressableManager.Instance.LoadAssetAsync<SpriteAtlas>("Atlas",
                (asyncOperationHandle) =>
                {
                    var spriteAtlas = asyncOperationHandle.Result;
                    if (spriteAtlas != null)
                        _spriteAtlasDic?.TryAdd(spriteAtlas.name, spriteAtlas);
                });
        }

        public Sprite GetSprite(string altasName, string spriteName)
        {
            if (_spriteAtlasDic == null)
                return null;

            if (_spriteAtlasDic.ContainsKey(altasName))
                return _spriteAtlasDic[altasName]?.GetSprite(spriteName);

            return null;
        }

        public Sprite GetCharacterSprite(string spriteName)
        {
            return GetSprite("CharacterUI", spriteName);
        }
    }
}


