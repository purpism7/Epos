using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Info
{
    [Serializable]
    public class Formation
    {
        public int Index = 0;
        public int[,] CharacterIds = new int[2, 3];
    }
}

