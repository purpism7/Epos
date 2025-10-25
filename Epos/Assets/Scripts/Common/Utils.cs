using UnityEngine;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Common
{
    public static class Utils
    {
        public static void DrawCircle(Vector3 position, float radius, Color color, float duration)
        {
            float angleStep = 360f / 40;
            Vector3 prevPoint = Vector3.zero;

            for (int i = 0; i <= 40; i++)
            {
                float currentAngle = angleStep * i * Mathf.Deg2Rad;

                // 삼각함수를 이용해 원 위의 좌표 계산
                float x = position.x + radius * Mathf.Cos(currentAngle);
                float y = position.y + radius * Mathf.Sin(currentAngle);

                // 2D 환경을 위해 Z축은 중심의 Z축을 사용
                Vector3 currentPoint = new Vector3(x, y, position.z); 

                // 첫 번째 점이 아닐 때만 선을 그립니다.
                if (i > 0)
                {
                    // Debug.DrawLine: 한 프레임 동안만 선을 그립니다.
                    // 따라서 LateUpdate/Update에서 매번 호출해야 계속 보입니다.
                    Debug.DrawLine(prevPoint, currentPoint, color);
                }
            
                prevPoint = currentPoint;
            }
        }
    }
}

