using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creature;

namespace Parts
{
    public class Forces : Part
    {
        // 임시. 캐릭터 데이터 추가 후 생성 방식으로 변경.
        [SerializeField]
        private Character[] characters = null;

        public List<Character> CharacterList { get; private set; } = new List<Character>() { null, null, null, null, null, null };

        private void Awake()
        {
            var height = 2*Camera.main.orthographicSize;
            var width = height*Camera.main.aspect;
            Debug.Log(height);
            Debug.Log(width);
            
            if (characters != null)
            {
                if (CharacterList == null)
                {
                    CharacterList = new();
                    CharacterList.Clear();
                }

                for (int i = 0; i < characters.Length; ++i)
                {
                    var character = characters[i];
                    if (character.Position > 0)
                    {
                        if (CharacterList?.Count > character.Position - 1)
                            CharacterList[character.Position - 1] = character;
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

           
        }
    }
}

