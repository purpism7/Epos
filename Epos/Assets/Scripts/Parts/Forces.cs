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

        public List<Character> CharacterList { get; private set; } = null;

        private void Awake()
        {
            if (characters != null)
            {
                if (CharacterList == null)
                {
                    CharacterList = new();
                    CharacterList.Clear();
                }

                for (int i = 0; i < 6; ++i)
                {
                    CharacterList?.Add(null);
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

