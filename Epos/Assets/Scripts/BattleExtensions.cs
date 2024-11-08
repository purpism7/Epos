using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Ability;
using Creature;

public static class BattleExtensions
{
    public static List<ICombatant> GetTargetList(this ICombatant attacker, List<ICombatant> iCombatantList, Skill skill)
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

            if (skill.SameTeam)
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
        
        // 스킬 사용 조건에 맞춰 Target 이 지정되어야함
        if (skill.ESkillTarget == Type.ESkillTarget.FarOne ||
            skill.ESkillTarget == Type.ESkillTarget.NearOne)
        {
            var target = targetList.FirstOrDefault();
            targetList.Clear();
            targetList.Add(target);
        }

        return targetList;
    }
}
