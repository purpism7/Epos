using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common;

namespace GameSystem
{
    public class Grid : MonoBehaviour
    {
        private const float CellSize = 8f;
        
        [SerializeField] 
        private int row = 0; // 행
        [SerializeField] 
        private int column = 0; // 열
        
        [SerializeField]
        private List<float> rowOffsetXList = new();
        
        [SerializeField] 
        private GameObject cellGameObj = null;

        private List<Transform> _cellTmList = null;


        public void Initialize()
        {
            if (_cellTmList == null)
            {
                _cellTmList = new();
                _cellTmList?.Clear();
            }
            
            var childTms = GetComponentsInChildren<Transform>(true);
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
            
            _cellTmList?.AddRange(resChildTmList);
        }
        
#if UNITY_EDITOR
        public void RePosition()
        {
            if (_cellTmList.IsNullOrEmpty())
                return;

            int index = 0;
            for (int i = column - 1; i >= 0; --i)
            {
                for (int j = row - 1; j >= 0; --j)
                {
                    if(_cellTmList.Count <= index)
                        continue;

                    SetCellPosition(_cellTmList[index], j, i);
                    _cellTmList[index].name = $"[{j},{i}]-{index}";
                    
                    ++index;
                }
            }
        }
        
        public void Generate()
        {
            if (_cellTmList == null)
                _cellTmList = new();
            
            _cellTmList?.Clear();
            
            transform.RemoveAllChild();
            
            for (int i = column - 1; i >= 0; --i)
            {
                for (int j = row - 1; j >= 0; --j)
                {
                    var cell = Instantiate(cellGameObj, transform);
                    if(!cell ||
                       !cell.transform)
                        continue;

                    var boxCollider = cell.GetComponent<BoxCollider>();
                    if (boxCollider != null)
                        boxCollider.size = Vector3.one * CellSize;

                    SetCellPosition(cell.transform, j, i);
                   
                    cell.name = $"[{j},{i}]-{_cellTmList?.Count}";
                    
                    _cellTmList?.Add(cell.transform);
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
            float halfCellSize = CellSize * 0.5f;
            return index * CellSize + halfCellSize;
        }
#endif

        public Transform GetCellTm(int index)
        {
            if (_cellTmList.IsNullOrEmpty())
                return null;

            return _cellTmList[index];
        }
    }
}

