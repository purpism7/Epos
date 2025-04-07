using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Creature;

namespace Parts
{
    public class PartyLocation : UI.Component
    {
        // 임시. 캐릭터 데이터 추가 후 생성 방식으로 변경.
        [SerializeField]
        private int[] characterIds = null;

        [SerializeField] private Transform characterRootTm = null;
        [SerializeField] private GameSystem.Grid grid = null;

        public List<int> CharacterList => characterIds?.ToList(); 
        
        public Transform CharacterRootTm => characterRootTm;
        
        public override void Initialize()
        {
            base.Initialize();
            
            grid?.Initialize();
        }

        public Vector3 GetPartyPosition(int index)
        {
            if(grid == null)
                return Vector3.zero;

            var cellTm = grid.GetCellTm(index);
            if(!cellTm)
                return Vector3.zero;
            
            return cellTm.position;
        }
    }
}

