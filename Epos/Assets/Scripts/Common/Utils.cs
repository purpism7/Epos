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
            Vector3 prevPoint = position + new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

            for (int i = 1; i <= 40; i++)
            {
                float rad = Mathf.Deg2Rad * (i * angleStep);
                Vector3 nextPoint = position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

                Debug.DrawLine(prevPoint, nextPoint, color, duration);

                prevPoint = nextPoint;
                prevPoint.z = -1f;
            }
        }
    }
}

