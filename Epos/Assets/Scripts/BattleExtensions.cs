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
                if (attacker.ETeam == iCombatant.ETeam)
                    targetList.Add(iCombatant);
            }
            else
            {
                if (attacker.ETeam != iCombatant.ETeam)
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

    public static bool IsCircle(this ICombatant attacker, ICombatant targetICombatant, float range)
    {
        if (attacker == null)
            return false;

        var targetIActor = targetICombatant?.IActor;
        if (targetIActor == null)
            return false;

        var targetPosition = targetIActor.Transform.position;
        var targetCollider = targetIActor.Collider;
        if (targetCollider != null)
            targetPosition = targetCollider.ClosestPoint(attacker.Transform.position);

        var distance = Vector2.Distance(attacker.Transform.position, targetPosition);
        return distance <= range;
    }

    public static bool IsSector(this ICombatant attacker, ICombatant targetICombatant, float range)
    {
        if (attacker == null)
            return false;

        if (targetICombatant == null)
            return false;

        if (!attacker.IsCircle(targetICombatant, range))
            return false;

        Vector2 direction = (targetICombatant.IActor.Transform.position - attacker.Transform.position).normalized;
        Vector2 forwardDirection = attacker.Transform.right; // 플레이어의 앞 방향 (2D에서는 주로 right)
        float angle = Vector2.Angle(forwardDirection, direction);
        //Debug.Log(angle);
        return angle <= 60f / 2f;

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

            var distance = Vector2.Distance(iCombatantList[i].IActor.Transform.position, tm.position);
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
