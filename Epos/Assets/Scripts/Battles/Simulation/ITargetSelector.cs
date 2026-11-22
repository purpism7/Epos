using System.Collections.Generic;

namespace Battles.Simulation
{
    /// <summary>
    /// 표적 고르기 자리(판단 6단계가 나중에 들어올 곳). 시뮬이 판단 차례에만 부른다(DecisionSec 주기마다, 또는 지금 표적을 쓸 수 없게 됐을 때).
    /// 지킬 것: 틱 시작 때의 상태만 읽는다 · 아무것도 바꾸지 않는다 · 난수 · 시계 · 전역 값을 쓰지 않는다 ·
    /// 같은 입력 → 같은 답 · 동점은 번호가 작은 쪽 · 힙을 쓰지 않는다(판단 차례마다 불린다).
    /// self.TargetId(지금 표적)를 읽어 «바꾸려면 더 나아야 한다»(히스테리시스)를 넣을 수 있다.
    /// </summary>
    public interface ITargetSelector
    {
        /// <param name="self">고르는 유닛(전투 불능 아님 · 이동 입력 없음 · 공격할 수 있음).</param>
        /// <param name="units">모든 유닛(번호 오름차순). 전투 불능 · 같은 편도 들어 있다.</param>
        /// <returns>겨눌 유닛 번호. 없으면 BattleUnit.NoId. 겨눌 수 없는 번호를 돌려주면 시뮬이 NoId 로 본다.</returns>
        int SelectTarget(BattleUnit self, IReadOnlyList<BattleUnit> units);
    }
}
