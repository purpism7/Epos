using UnityEngine;

using Battles.Simulation;

namespace Battles
{
    /// <summary>
    /// 전투 규칙의 인스펙터 값(⚑ 임시). GameManager 프리팹(Assets/Resources/GameManager.prefab)에서 고친다.
    /// 플레이를 시작할 때 한 번 순수 시뮬 값(BattleRules)으로 바뀐다 — 플레이 중에 고친 값은 다음 플레이부터 반영된다.
    /// </summary>
    [System.Serializable]
    public sealed class BattleRulesSettings
    {
        [Tooltip("1초에 도는 전투 틱 수. 판정은 이 고정 간격으로만 나아가고, 그림은 프레임마다 시뮬 결과를 따라간다(보간 없음). 1~1000. ⚑ 60 = 1/60초")]
        [Min(1)] [SerializeField] private int ticksPerSecond = 60;

        [Tooltip("표적을 다시 고르는 주기(초). 표적이 전투 불능이 되거나 없으면 주기를 기다리지 않고 바로 고른다. 최소 1틱. ⚑ 0.4")]
        [Min(0f)] [SerializeField] private float decisionSec = 0.4f;

        public BattleRules ToBattleRules()
        {
            return new BattleRules(ticksPerSecond, decisionSec);
        }
    }
}
