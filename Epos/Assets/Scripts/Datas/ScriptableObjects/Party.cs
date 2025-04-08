using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Datas.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Party")]
    [System.Serializable]
    public class Party : ScriptableObject
    {
        public int Id = 0;

        [Serializable]
        public class PositionInfo
        {
            public int Position = 0;
            public int CharacterId = 0;
        }
        
        public PositionInfo[] PositionInfos = null;
    }
}