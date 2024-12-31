using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using GameSystem;


namespace Creator
{
    public class CharacterCreator<T> where T : Creature.Character
    {
        private int _id = 0;

        public CharacterCreator<T> SetId(int id)
        {
            _id = id;
            return this;
        }
        
        public T Create
        {
            get
            {
                GameObject loadGameObj = AddressableManager.Instance.LoadAssetByNameAsync<GameObject>(_id.ToString());
                var gameObj = GameObject.Instantiate(loadGameObj);
                if (!gameObj)
                    return null;
                
                var t = gameObj.GetComponent<T>();
                Debug.Log(t);
                
                
                return t;
                // return 
            }
        }
    }
}

