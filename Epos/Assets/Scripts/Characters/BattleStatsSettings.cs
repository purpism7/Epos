using UnityEngine;

using Battles.Simulation;

namespace Characters
{
    /// <summary>
    /// 캐릭터 한 명의 전투 능력치 — 인스펙터에서 고치는 ⚑ 임시값(캐릭터 · 프리팹마다).
    /// 플레이를 시작할 때 BattleManager 가 한 번 읽어 순수 시뮬 값(BattleUnitStats)으로 바꾼다. 플레이 중에 고친 값은 다음 플레이부터 반영된다.
    /// 시간 값은 1/60초(틱) 단위로 반올림된다.
    /// </summary>
    [System.Serializable]
    public sealed class BattleStatsSettings
    {
        [Tooltip("최대 HP. 0 이 되면 전투 불능: 사라지지 않고 자리에 남지만 움직이거나 공격하지 않고 표적이 되지 않는다. ⚑ 338")]
        [Min(1)] [SerializeField] private int maxHp = 338;

        [Tooltip("기본 공격 한 번의 피해. 0 이면 표적을 고르지도 공격하지도 않는다(가만히 맞는 허수아비). ⚑ 14")]
        [Min(0)] [SerializeField] private int attackDamage = 14;

        [Tooltip("공격 사거리(유니티 단위). 두 캐릭터 원점 사이 거리로 잰다. 쫓아갈 때는 이보다 0.05 안쪽에서 멈춘다(표적이 나를 쫓아오는 중이 아니면 표적의 1틱 이동 거리만큼 더 — 달아나는 표적도 따라잡게). 0.1 이상. ⚑ 2.5")]
        [Min(0.1f)] [SerializeField] private float attackRange = 2.5f;

        [Tooltip("이동 속도(유니티 단위/초). 키 이동과 쫓아가기에 같이 쓴다. 0 이면 움직이지 않는다. ⚑ 5")]
        [Min(0f)] [SerializeField] private float moveSpeed = 5f;

        [Tooltip("휘두르기 시작 → 피해(초). 스파인 이벤트가 없어 이 값이 맞는 때다. 동작 길이보다 짧아야 한다(넘으면 동작 끝 1틱 전으로 자른다). ⚑ 0.1")]
        [Min(0f)] [SerializeField] private float attackHitSec = 0.1f;

        [Tooltip("휘두르기 동작 길이(초). 이 동안 서서 휘두르고 표적을 바꾸지 않는다(이동 키를 누르면 끊긴다). 그림(Skill01) 길이와 맞춘다. ⚑ 0.2 = SD 기사 Skill_01")]
        [Min(0f)] [SerializeField] private float attackDurationSec = 0.2f;

        [Tooltip("휘두르기 시작 → 다음 휘두르기 시작(초). 동작 길이보다 짧으면 동작 길이를 쓴다. 남는 시간은 표적을 보고 서서 기다린다. ⚑ 0.5")]
        [Min(0f)] [SerializeField] private float attackIntervalSec = 0.5f;

        public BattleUnitStats ToBattleUnitStats()
        {
            return new BattleUnitStats(maxHp, attackDamage, attackRange, moveSpeed, attackHitSec, attackDurationSec, attackIntervalSec);
        }
    }
}
