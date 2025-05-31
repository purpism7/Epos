using System.Collections;
using System.Collections.Generic;
using Creator;
using UnityEngine;

using Creature;

namespace Entities
{
    public interface ICharacterManager : IManager
    {
        T Create<T>(int id, Transform rootTm) where T : Creature.Character;
    }

    public class Character : ICharacterManager
    {
        private Dictionary<int, Creature.Character> _cachedDic = null;
        
        public IGeneric Initialize()
        {
            return this;
        }

        void IGeneric.ChainUpdate()
        {
            
        }
        
        void IGeneric.ChainLateUpdate()
        {
            
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
                character = new CharacterCreator<T>()
                    .SetId(id)
                    .SetRoot(rootTm)
                    .Create;
            }

            var t = character as T;
            if (t == null)
                return null;
            
            character.SetActive(true);
            t.Initialize();

            return t;
        }
    }
}

