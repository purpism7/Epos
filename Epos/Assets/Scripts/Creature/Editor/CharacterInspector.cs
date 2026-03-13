using UnityEditor;
using UnityEngine;

using Creature;

[CustomEditor(typeof(Character), true)]
public class CharacterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var character = target as Character;
        if (character == null)
            return;

        var iActor = character as IActor;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Is Alive = {iActor?.IsAlive ?? false}");

        if (Application.isPlaying)
        {
            // 현재 Act
            var currentActName = character.ActController?.GetCurrentActName() ?? "—";
            EditorGUILayout.LabelField("Current Act", currentActName);

            // 현재 애니메이션
            var skeletonAnimation = character.SkeletonAnimation;
            var currentAnimationName = "—";
            if (skeletonAnimation?.AnimationState != null)
            {
                var trackEntry = skeletonAnimation.AnimationState.GetCurrent(0);
                currentAnimationName = trackEntry?.Animation?.Name ?? "—";
            }
            EditorGUILayout.LabelField("Current Animation", currentAnimationName);
        }
        else
        {
            EditorGUILayout.HelpBox("플레이 모드에서 Current Act와 Animation을 확인할 수 있습니다.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

        if (Application.isPlaying)
            Repaint();
    }
}
