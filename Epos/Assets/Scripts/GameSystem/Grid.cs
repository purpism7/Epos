using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] 
        private int row = 0; // 행
        [SerializeField] 
        private int column = 0; // 열
        
        [SerializeField]
        private List<float> rowOffsetXList = new();
        
        [SerializeField] 
        private GameObject cellGameObj = null;

#if UNITY_EDITOR
        public void RePosition()
        {
            var childTms = GetComponentsInChildren<Transform>(true);
            if (childTms.IsNullOrEmpty())
                return;

            var resChildTmList = new List<Transform>();
            resChildTmList.Clear();
            
            foreach (var childTm in childTms)
            {
                if(!childTm || childTm.parent != transform)
                    continue;
                
                if(childTm == transform)
                    continue;
                    
                resChildTmList.Add(childTm);
            }

            int index = 0;
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    if(resChildTmList.Count <= index)
                        continue;

                    SetCellPosition(resChildTmList[index], i, j);
                    resChildTmList[index].name = $"[{i},{j}]";
                    
                    ++index;
                }
            }
        }
        
        public void Generate()
        {
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    var cell = Instantiate(cellGameObj, transform);
                    if(!cell ||
                       !cell.transform)
                        continue;

                    SetCellPosition(cell.transform, i, j);
                   
                    cell.name = $"[{j},{i}]";
                }
            }
        }

        private void SetCellPosition(Transform cellTm, int row, int column)
        {
            if (!cellTm)
                return;
            
            var x = GetCellPos(column);
            var y = GetCellPos(row);
            
            float offsetX = 0;
            if (!rowOffsetXList.IsNullOrEmpty() &&
                rowOffsetXList.Count > row)
                offsetX = rowOffsetXList[row];

            cellTm.localPosition = new Vector3(x + offsetX , y, 0);
        }

        private float GetCellPos(int index)
        {
            float cellSize = 8f;
            float halfCellSize = cellSize * 0.5f;
                
            return index * cellSize + halfCellSize;
        }
#endif
    }
}

