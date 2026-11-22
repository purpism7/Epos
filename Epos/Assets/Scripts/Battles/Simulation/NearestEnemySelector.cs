using System.Collections.Generic;

namespace Battles.Simulation
{
    /// <summary>
    /// 임시 표적 고르기(⚑): 겨눌 수 있는 상대(다른 편 · 전투 불능 아님) 가운데 가장 가까운 유닛. 거리가 같으면 번호가 작은 쪽.
    /// 판단 6단계가 들어오면 이 클래스를 바꿔 끼운다(BattleManager 에서 만드는 한 줄).
    /// </summary>
    public sealed class NearestEnemySelector : ITargetSelector
    {
        public int SelectTarget(BattleUnit self, IReadOnlyList<BattleUnit> units)
        {
            if (self == null || units == null)
                return BattleUnit.NoId;

            int bestId = BattleUnit.NoId;
            float bestSqrDistance = float.PositiveInfinity;

            for (int i = 0; i < units.Count; i++)
            {
                var candidate = units[i];
                if (!self.CanTarget(candidate))
                    continue;

                float sqrDistance = (candidate.Position - self.Position).sqrMagnitude;

                // 동점은 목록 순서에 기대지 않고 번호로 가른다.
                if (sqrDistance < bestSqrDistance ||
                    (sqrDistance == bestSqrDistance && candidate.Id < bestId))
                {
                    bestSqrDistance = sqrDistance;
                    bestId = candidate.Id;
                }
            }

            return bestId;
        }
    }
}
