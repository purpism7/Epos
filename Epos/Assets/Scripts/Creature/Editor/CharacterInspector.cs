using UnityEditor;

using Creature;

[CustomEditor(typeof(Monster))]
public class CharacterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var character = target as Monster;
        if (character == null)
            return;

        var iActor = character as IActor;
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Is Alive = {iActor.IsAlive}");
        EditorGUILayout.EndVertical();
    }
}
