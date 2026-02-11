using System.Collections.Generic;
using UnityEngine;

using Creature;

public static class BattleExtensions
{
    public static List<ICombatant> GetTargetList(this ICombatant attacker, List<ICombatant> iCombatantList, Ability.ISkill iSkill)
    {
        if (attacker == null)
            return null;

        if (iCombatantList == null)
            return null;

        var skillData = iSkill?.SkillData;
        if (skillData == null)
            return null;

        List<ICombatant> targetList = new();
        targetList.Clear();

        foreach (var iCombatant in iCombatantList)
        {
            if (iCombatant == null)
                continue;

            if (!iCombatant.IActor.IsActivate)
                continue;

            if (!iCombatant.IActor.IsAlive)
                continue;

            if (skillData.SameTeam)
            {
                if (attacker.TeamType == iCombatant.TeamType)
                    targetList.Add(iCombatant);
            }
            else
            {
                if (attacker.TeamType != iCombatant.TeamType)
                    targetList.Add(iCombatant);
            }
        }

        if (targetList.IsNullOrEmpty())
            return null;

        //switch(skillData.ESkillTarget)
        //{
        //    case ESkillTarget.NearOne:
        //        {


        //            break;
        //        }

        //    case ESkillTarget.FarOne:
        //        {
        //            var target = targetList.FirstOrDefault();
        //            targetList.Clear();
        //            targetList.Add(target);

        //            break;
        //        }
        //}

        //var target = FindClosestICombatant(targetList, attacker);
        //targetList.Clear();
        //targetList.Add(target);

        return targetList;
    }

    public static bool IsCircle(this ICombatant attacker, ICombatant target, float range)
    {
        if (attacker == null)
            return false;

        var targetIActor = target?.IActor;
        if (targetIActor == null)
            return false;

        var targetPosition = targetIActor.Transform.position;
        var targetCollider = targetIActor.Collider;
        // if (targetCollider != null)
        //     targetPosition = targetCollider.ClosestPoint(attacker.Transform.position);
        
        var difference = targetPosition - attacker.Transform.position;
        var distance = difference.magnitude;
        float sqrDistance = Vector3.SqrMagnitude(targetPosition - attacker.Transform.position);

        // var distance = Vector2.Distance(attacker.Transform.position, targetPosition);
        return sqrDistance < range * range;
    }

    /// <summary>부채꼴(섹터) 범위 내에 타겟이 있는지 검사. direction을 중심으로 ±(angle/2)도, 거리 range 이내.</summary>
    public static bool IsSector(this ICombatant attacker, ICombatant target, Vector2 direction, float range, float angle)
    {
        if (attacker == null || target == null)
            return false;

        Vector2 toTarget = target.Transform.position - attacker.Transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        if (sqrDist > range * range)
            return false;

        // 거리 0: 같은 위치 → 부채꼴 내로 간주 (normalized 시 (0,0) 방지)
        const float kEpsilonSqr = 0.0001f;
        if (sqrDist < kEpsilonSqr)
            return true;

        float halfAngle = angle * 0.5f;
        Vector2 dirToTarget = toTarget.normalized;
        Vector2 forwardDir = direction.normalized;

        float angleToTarget = Mathf.Abs(Vector2.SignedAngle(forwardDir, dirToTarget));
        return angleToTarget <= halfAngle;
        // Vector2 upDirection = new Vector2(Mathf.Cos(upAngleRad), Mathf.Sin(upAngleRad));
        // Vector3 upEndPos = attacker.Transform.position + (Vector3)upDirection * range;
        // // Debug.DrawLine(attacker.Transform.position, upEndPos, Color.magenta);
        //
        // float angleToTarget = Mathf.Abs(Vector2.SignedAngle(upDirection, direction));
        // if (angleToTarget > degreeRad)
        //     return false;
        //
        // // --- 5. 아랫방향 45도 계산 ---
        // float downAngleRad = baseAngleRad - degreeRad;
        // Vector2 downDirection = new Vector2(Mathf.Cos(downAngleRad), Mathf.Sin(downAngleRad));
        // Vector3 dowEndPos = attacker.Transform.position + (Vector3)downDirection * range;
        // // Debug.DrawLine(attacker.Transform.position, dowEndPos, Color.magenta);
        //
        // angleToTarget = Mathf.Abs(Vector2.SignedAngle(downDirection, direction));
        // if (angleToTarget > degreeRad)
        //     return false;
        // // Vector3 direction = target.Transform.position- attacker.Transform.position.normalized;
        //
        // return true;
        // return baseAngleRad * Mathf.Rad2Deg < angle * 0.5f;
    

    
        // if (dot >= requiredCos)
        // {
        //     return true; // 범위 내에 있음
        // }
        
        // return false;
        
        //
        // Vector2 forwardDirection = attacker.Transform.right; // �÷��̾��� �� ���� (2D������ �ַ� right)
        // float angle = Vector2.Angle(forwardDirection, direction);
        // //Debug.Log(angle);
        // return angle <= 60f / 2f;

        //float dot = Vector2.Dot(attacker.Transform.up, direction);
        //var alertThreshold = Mathf.Cos(90f * 0.5f * Mathf.Deg2Rad);

        //return dot >= alertThreshold;
    }

    public static ICombatant FindClosestICombatant(this Transform tm, List<ICombatant> iCombatantList)
    {
        if (!tm)
            return null;

        if (iCombatantList.IsNullOrEmpty())
            return null;

        ICombatant closestIComtant = null;
        float closestDistance = 99999f;
        for (int i = 0; i < iCombatantList.Count; ++i)
        {
            var iActor = iCombatantList[i]?.IActor;
            if (iActor == null)
                continue;

            if (!iActor.IsAlive)
                continue;

            var distance = Vector2.Distance(iActor.Transform.position, tm.position);
            if (closestIComtant == null ||
                closestDistance > distance)
            {
                closestIComtant = iCombatantList[i];
                closestDistance = distance;
            }
        }

        return closestIComtant;
    }

    public static ICombatant FindFarthestICombatant(this ICombatant iCombatant, List<ICombatant> iCombatantList)
    {
        if (iCombatantList.IsNullOrEmpty())
            return null;

        ICombatant farthestIComtant = null;
        float farthestDistance = 0;
        for (int i = 0; i < iCombatantList.Count; ++i)
        {
            if (iCombatantList[i] == null)
                continue;

            var distance = Vector2.Distance(iCombatantList[i].IActor.Transform.position, iCombatant.IActor.Transform.position);
            if (farthestIComtant == null ||
                farthestDistance < distance)
            {
                farthestIComtant = iCombatantList[i];
                farthestDistance = distance;
            }
        }

        return farthestIComtant;
    }
}
