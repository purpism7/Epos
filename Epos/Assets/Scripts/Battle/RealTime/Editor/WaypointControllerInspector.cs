using UnityEditor;
using UnityEngine;

using Battle;
using Battle.RealTime;

namespace Battle.RealTime.Editor
{
    [CustomEditor(typeof(WaypointController))]
    public class WaypointControllerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var controller = target as WaypointController;
            if (controller == null)
                return;

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Waypoint 상태", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                var waypoint = controller.Waypoint;
                if (waypoint != null)
                {
                    EditorGUILayout.LabelField("현재 Waypoint", waypoint.name);
                    EditorGUILayout.LabelField("위치", waypoint.Position.ToString("F2"));
                    EditorGUILayout.LabelField("생존 몬스터 수", waypoint.AliveMonsterCount.ToString());
                    EditorGUILayout.LabelField("생존 몬스터 있음", waypoint.HasAliveMonsters ? "Yes" : "No");
                }
                else
                {
                    EditorGUILayout.LabelField("현재 Waypoint", "— (없음)");
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("HasAliveMonsters", controller.HasAliveMonsters ? "Yes" : "No");
                EditorGUILayout.LabelField("남은 Waypoint 수", controller.RemainingWaypointCount.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("플레이 모드에서 Waypoint 상태를 확인할 수 있습니다.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            if (Application.isPlaying)
                Repaint();
        }
    }
}
