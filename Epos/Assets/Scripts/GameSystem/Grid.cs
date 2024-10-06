using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] 
        private int row = 0;
        [SerializeField] 
        private int column = 0;
        
        [SerializeField] 
        private GameObject cellGameObj = null;
        
        public void Generate()
        {
            float cellSize = 10f;
            float halfCellSize = cellSize * 0.5f;
            
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    var cell = Instantiate(cellGameObj, transform);
                    if(!cell ||
                       !cell.transform)
                        continue;

                    var x = i * cellSize + halfCellSize;
                    var y = j * cellSize + halfCellSize;

                    cell.transform.localPosition = new Vector3(x , y, 0);
                }
            }
        }
    }
}

