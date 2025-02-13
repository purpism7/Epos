using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Datas.ScriptableObjects
{
    [CreateAssetMenu]
    public class Skill : ScriptableObject
    {
        public int ActivePoint = 0;
        public int PassivePoint = 0;
    }
}

