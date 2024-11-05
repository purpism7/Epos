using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Field
    {
        public int FieldId { get; private set; } = 0;
        public int FieldPointId { get; private set; } = 0;
        
        public int[,] CharacterIds { get; private set; } = null;

        public Field(int fieldPointId)
        {
            FieldPointId = fieldPointId;
            
            switch (fieldPointId)
            {
                case 1:
                {
                    CharacterIds = new int[2, 3];
                    CharacterIds[1, 1] = 90001;
                    
                    break;
                }
            }
        }
    }
}

