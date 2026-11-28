using System.Collections.Generic;

namespace Battles.Simulation
{
    /// <summary>판의 결과(아군 쪽에서 본다). 기록 · 다시 보기에 쓰일 수 있으므로 있는 값을 바꾸지 말고 뒤에 더한다.</summary>
    public enum BattleOutcome
    {
        /// <summary>아직 양쪽 모두 서 있는 유닛이 있다.</summary>
        Ongoing = 0,
        /// <summary>적이 모두 전투 불능.</summary>
        Victory = 1,
        /// <summary>아군이 모두 전투 불능.</summary>
        Defeat = 2,
        /// <summary>같은 틱에 양쪽 모두 마지막 유닛이 전투 불능(동시 타격).</summary>
        Draw = 3,
    }

    /// <summary>
    /// 한 판의 승패를 가르는 순수 규칙. 유닛 상태를 읽기만 한다(같은 상태 → 같은 답).
    /// 한쪽 편이 아예 없으면(등록된 유닛 0) 겨루는 판이 아니므로 판정하지 않는다 — 기사 혼자 움직여 보는 장면이 곧바로 «승리»로 끝나지 않게.
    /// </summary>
    public static class BattleOutcomeJudge
    {
        public static BattleOutcome Evaluate(IReadOnlyList<BattleUnit> units)
        {
            if (units == null)
                return BattleOutcome.Ongoing;

            int allyCount = 0;
            int allyStanding = 0;
            int enemyCount = 0;
            int enemyStanding = 0;

            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (unit == null)
                    continue;

                if (unit.Team == BattleTeam.Ally)
                {
                    allyCount++;
                    if (!unit.IsDown)
                        allyStanding++;
                }
                else
                {
                    enemyCount++;
                    if (!unit.IsDown)
                        enemyStanding++;
                }
            }

            if (allyCount == 0 || enemyCount == 0)
                return BattleOutcome.Ongoing;

            if (allyStanding == 0)
                return enemyStanding == 0 ? BattleOutcome.Draw : BattleOutcome.Defeat;

            return enemyStanding == 0 ? BattleOutcome.Victory : BattleOutcome.Ongoing;
        }
    }
}
