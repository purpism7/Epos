using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Field
    {
        public int Id { get; private set; } = 0;
        public int[,] CharacterIds { get; private set; } = null;

        public Field(int id, int[,] characterIds)
        {
            Id = id;
            CharacterIds = characterIds;
        }
    }
}

