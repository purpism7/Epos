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

            if (!iCombatant.Actor.IsActivate)
                continue;

            if (!iCombatant.Actor.IsAlive)
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

        return targetList;
    }

    public static bool IsCircle(this ICombatant attacker, ICombatant target, float range)
    {
        if (attacker == null)
            return false;

        var targetIActor = target?.Actor;
        if (targetIActor == null)
            return false;

        var targetPosition = targetIActor.Transform.position;
        var targetCollider = targetIActor.Collider;
        // if (targetCollider != null)
        //     targetPosition = targetCollider.ClosestPoint(attacker.Transform.position);
        
        var difference = targetPosition - attacker.Transform.position;
        var distance = difference.magnitude;
        float sqrDistance = Vector3.SqrMagnitude(targetPosition - attacker.Transform.position);

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
    }

    public static ICombatant FindClosestICombatant(this Transform tm, List<ICombatant> combatantList)
    {
        if (!tm)
            return null;

        if (combatantList.IsNullOrEmpty())
            return null;

        ICombatant closestIComtant = null;
        float closestDistance = 99999f;
        for (int i = 0; i < combatantList.Count; ++i)
        {
            var iActor = combatantList[i]?.Actor;
            if (iActor == null)
                continue;

            if (!iActor.IsAlive)
                continue;

            var distance = Vector2.Distance(iActor.Transform.position, tm.position);
            if (closestIComtant == null ||
                closestDistance > distance)
            {
                closestIComtant = combatantList[i];
                closestDistance = distance;
            }
        }

        return closestIComtant;
    }

    public static ICombatant FindFarthestICombatant(this ICombatant iCombatant, List<ICombatant> combatantList)
    {
        if (combatantList.IsNullOrEmpty())
            return null;

        ICombatant farthestIComtant = null;
        float farthestDistance = 0;
        for (int i = 0; i < combatantList.Count; ++i)
        {
            if (combatantList[i] == null)
                continue;

            var distance = Vector2.Distance(combatantList[i].Actor.Transform.position, iCombatant.Actor.Transform.position);
            if (farthestIComtant == null ||
                farthestDistance < distance)
            {
                farthestIComtant = combatantList[i];
                farthestDistance = distance;
            }
        }

        return farthestIComtant;
    }
}
