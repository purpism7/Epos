using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSystem.Grid))]
public class GridInspector : Editor
{
    private SerializedProperty _rowProperty;
    private SerializedProperty _rowOffsetXListProperty;

    private void OnEnable()
    {
        _rowProperty = serializedObject.FindProperty("row");
        _rowOffsetXListProperty = serializedObject.FindProperty("rowOffsetXList");
        // _columnProperty = serializedObject.FindProperty("column");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var grid = target as GameSystem.Grid;
        if (grid == null)
            return;
        
        serializedObject.Update();

        // SerializedProperty를 통해 private 필드 값 표시
        // EditorGUILayout.PropertyField(_rowProperty);
        if (_rowProperty.intValue > 0)
        {
            if (_rowOffsetXListProperty != null)
                _rowOffsetXListProperty.arraySize = _rowProperty.intValue;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // grid.Row = EditorGUILayout.IntField("Row", grid.Row);


        if (GUILayout.Button("Generate"))
            grid.Generate();
        
        if (GUILayout.Button("RePosition"))
            grid.RePosition();
    }
}
