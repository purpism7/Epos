using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;

namespace Datas.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Strategy")]
    [System.Serializable]
    public class Strategy : ScriptableObject
    {
        public int Id = 0;
        public string StrategyTypeName = string.Empty;
        public string Description = string.Empty;
        public string EffectDescription = string.Empty;
        public string Phases = string.Empty;
    }
}

