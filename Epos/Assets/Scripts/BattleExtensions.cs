using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Creature;
using Common;

public static class BattleExtensions
{
    public static List<ICombatant> GetTargetList(this ICombatant attacker, List<ICombatant> iCombatantList, Ability.ISkill iSkill)
    {
        if (attacker == null)
            return null;
        
        if (iCombatantList == null)
            return null;
            
        List<ICombatant> targetList = new();
        targetList.Clear();
        
        foreach (var iCombatant in iCombatantList)
        {
            if(iCombatant == null)
                continue;
            
            if(!iCombatant.IActor.IsActivate)
                continue;

            if (iSkill.SameTeam)
            {
                if(attacker.ETeam == iCombatant.ETeam)
                    targetList.Add(iCombatant);
            }
            else
            {
                if(attacker.ETeam != iCombatant.ETeam)
                    targetList.Add(iCombatant);
            }
        }

        if (targetList.IsNullOrEmpty())
            return null;

        if (iSkill.ESkillTarget == ESkillTarget.NearOne)
        {
            var target = FindClosestICombatant(targetList, attacker);
            targetList.Clear();
            targetList.Add(target);
            
            return targetList;
        }
        
        // 스킬 사용 조건에 맞춰 Target 이 지정되어야함
        if (iSkill.ESkillTarget == ESkillTarget.FarOne)
        {
            var target = targetList.FirstOrDefault();
            targetList.Clear();
            targetList.Add(target);
        }

        return targetList;
    }
    
    private static ICombatant FindClosestICombatant(List<ICombatant> iCombatantList, ICombatant refICombatant)
    {
        if (iCombatantList.IsNullOrEmpty())
            return null;
        
        ICombatant closestEnemyIComtant = null;
        float closestDistance = 99999f;
        for (int i = 0; i < iCombatantList.Count; ++i)
        {
            if (iCombatantList[i] == null)
                continue;
        
            var distance = Vector2.Distance(iCombatantList[i].IActor.Transform.position, refICombatant.IActor.Transform.position);
            if (closestEnemyIComtant == null ||
                closestDistance > distance)
            {
                closestEnemyIComtant = iCombatantList[i];
                closestDistance = distance;
            }
        }
        
        return closestEnemyIComtant;
    }
}
