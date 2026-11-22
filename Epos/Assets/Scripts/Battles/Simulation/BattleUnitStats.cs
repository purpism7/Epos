namespace Battles.Simulation
{
    /// <summary>
    /// 유닛 하나의 전투 능력치(⚑ 임시값). 거리는 유니티 단위, 속도는 유니티 단위/초, 시간은 초.
    /// 유니티 속성을 붙이지 않는 순수 값이다. 인스펙터 값은 Characters.BattleStatsSettings(캐릭터마다)에서 온다.
    /// AddUnit 이 한 번 읽어 틱 단위로 바꾸고, 잘못된 값(음수 · NaN · 무한대)은 BattleUnit 이 안전한 값으로 고쳐 쓴다.
    /// </summary>
    public readonly struct BattleUnitStats
    {
        /// <summary>최대 HP(= 시작 HP). 1 이상. 0 이 되면 전투 불능.</summary>
        public int MaxHp { get; }

        /// <summary>기본 공격 한 번의 피해. 0 이하면 표적을 고르지도 공격하지도 않는다(허수아비).</summary>
        public int AttackDamage { get; }

        /// <summary>공격 사거리(두 유닛 위치 사이 거리). 0.1 이상.</summary>
        public float AttackRange { get; }

        /// <summary>이동 속도(유니티 단위/초). 키 이동과 쫓아가기에 같이 쓴다. 0 이면 움직이지 않는다.</summary>
        public float MoveSpeed { get; }

        /// <summary>휘두르기 시작 → 피해(초). 스파인 이벤트 대신 이 값이 맞는 때다. 동작 길이 - 1틱을 넘지 않는다.</summary>
        public float AttackHitSec { get; }

        /// <summary>휘두르기 동작 길이(초). 이 동안 서서 휘두르고 표적을 바꾸지 않는다. 최소 1틱.</summary>
        public float AttackDurationSec { get; }

        /// <summary>휘두르기 시작 → 다음 휘두르기 시작(초). 동작 길이보다 짧으면 동작 길이를 쓴다.</summary>
        public float AttackIntervalSec { get; }

        public BattleUnitStats(int maxHp, int attackDamage, float attackRange, float moveSpeed,
            float attackHitSec, float attackDurationSec, float attackIntervalSec)
        {
            MaxHp = maxHp;
            AttackDamage = attackDamage;
            AttackRange = attackRange;
            MoveSpeed = moveSpeed;
            AttackHitSec = attackHitSec;
            AttackDurationSec = attackDurationSec;
            AttackIntervalSec = attackIntervalSec;
        }
    }
}
