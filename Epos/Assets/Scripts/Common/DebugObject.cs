using System;
using UnityEngine;


public class DebugObject : MonoBehaviour
{
    public Transform originTm = null;
    public Vector3 targetPosition = Vector3.zero;
    private Vector3 previousPosition;
    private float offsetDistance = 5f;

    private void OnDrawGizmos()
    {
        if (originTm == null) return;

        // 1. 타겟의 가는 방향 벡터 정의
        // 2D 환경에서는 Rigidbody2D.velocity.normalized 또는 Transform.up을 사용합니다.
        // 여기서는 예시로 Transform.up (오브젝트의 로컬 앞쪽)을 사용합니다.
        Vector2 targetDirection = originTm.position - previousPosition;
        
        // 정규화된 벡터가 아니면 문제가 발생할 수 있으므로 항상 정규화합니다.
        if (targetDirection.sqrMagnitude < 0.0001f)
            return;
        
        targetDirection = targetDirection.normalized;
        
        // 왼쪽 수직 벡터 (반시계 90도): (-y, x)
        Vector2 leftVector = new Vector2(-targetDirection.y, targetDirection.x);
        
        // 오른쪽 수직 벡터 (시계 90도): (y, -x)
        Vector2 rightVector = new Vector2(targetDirection.y, -targetDirection.x);

        // --- 3. 최종 위치 계산 ---

        Vector3 targetPos = targetPosition;
        //Vector3 zOffset = new Vector3(0, 0, targetPos.z); // 2D 평면 유지를 위한 Z축

        // 1. 뒤쪽 위치 (Back Position)
        // 타겟 위치에서 (가는 방향 * 거리)를 뺍니다.
        Vector3 backPos = targetPos - ((Vector3)targetDirection * offsetDistance);

        // 2. 오른쪽 위치 (Right Position)
        // 타겟 위치에 (오른쪽 벡터 * 거리)를 더합니다.
        Vector3 rightPos = targetPos + ((Vector3)rightVector * offsetDistance);

        // 3. 왼쪽 위치 (Left Position)
        // 타겟 위치에 (왼쪽 벡터 * 거리)를 더합니다.
        Vector3 leftPos = targetPos + ((Vector3)leftVector * offsetDistance);

        previousPosition = originTm.position;
        // --- 4. 마커 위치 적용 및 시각화 ---
        
        var backPosition = new Vector3(backPos.x, backPos.y, targetPos.z);
        var rightPosition = new Vector3(rightPos.x, rightPos.y, targetPos.z);
        var leftPosition = new Vector3(leftPos.x, leftPos.y, targetPos.z);

        // (선택 사항) 에디터에서 시각화 (Gizmos)
        Debug.DrawLine(originTm.position, backPosition, Color.red);
        Debug.DrawLine(originTm.position, rightPosition, Color.green);
        Debug.DrawLine(originTm.position, leftPosition, Color.blue);
    }
}
