using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;


namespace Datas.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Party")]
    [System.Serializable]
    public class Party : ScriptableObject
    {
        public int Id = 0;
        [SerializeField] private ETeam eTeam = ETeam.None;
        
        [Serializable]
        public class PositionInfo
        {
            public int Position = 0;
            public int CharacterId = 0;
        }
        
        public PositionInfo[] PositionInfos = null;
        public ETeam ETeam => eTeam;
    }
}