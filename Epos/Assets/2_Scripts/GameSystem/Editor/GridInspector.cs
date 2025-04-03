using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSystem.Grid))]
public class GridInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var grid = target as GameSystem.Grid;
        if (grid == null)
            return;

        if (GUILayout.Button("Generate"))
        {
            grid.Generate();
        }
    }
}
