namespace Battles.Simulation
{
    /// <summary>
    /// 모든 유닛이 같이 쓰는 전투 규칙(⚑ 임시값). 시간은 초 — 시뮬을 만들 때 한 번 틱 수로 바꾼다.
    /// 유니티 속성을 붙이지 않는 순수 값이다. 인스펙터 값은 Battles.BattleRulesSettings(GameManager 프리팹)에서 온다.
    /// 잘못된 값은 BattleSimulation 이 안전한 값으로 고쳐 쓴다.
    /// </summary>
    public readonly struct BattleRules
    {
        /// <summary>1초에 도는 틱 수(⚑ 60 = 1틱 1/60초). 시뮬은 1 ~ 1000 으로 자른다.</summary>
        public int TicksPerSecond { get; }

        /// <summary>표적을 다시 고르는 주기(초, ⚑ 0.4). 최소 1틱. 지금 표적을 쓸 수 없게 되면 주기를 기다리지 않는다.</summary>
        public float DecisionSec { get; }

        public BattleRules(int ticksPerSecond, float decisionSec)
        {
            TicksPerSecond = ticksPerSecond;
            DecisionSec = decisionSec;
        }
    }
}
