using UnityEditor;
using UnityEngine;

using Creature;
using Creature.Action;

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

            // 스킬 상태
            var combatant = character.Combatant;
            if (combatant?.ISkillCtr != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Skill State", EditorStyles.boldLabel);
                var skillStates = combatant.ISkillCtr.GetSkillStates();
                for (int i = 0; i < skillStates.Count; i++)
                {
                    var info = skillStates[i];
                    var stateStr = info.State.ToString();
                    var cooldownStr = info.State == Ability.Skill.EState.Cooldown
                        ? $"{info.CooldownLeft:F1}s / {info.CooldownTotal:F0}s"
                        : "—";
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(info.Name, GUILayout.Width(120));
                    EditorGUILayout.LabelField(stateStr, GUILayout.Width(60));
                    EditorGUILayout.LabelField(cooldownStr);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("플레이 모드에서 Current Act, Animation, 스킬 상태를 확인할 수 있습니다.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

        if (Application.isPlaying)
            Repaint();
    }
}
