using UnityEngine;

namespace Battles.Simulation
{
    /// <summary>
    /// 이벤트 키 — 결과의 뜻만 담는다(그림 · 소리 · 문구 없음). 명세의 키(intent / hit / down)와 짝지어 둔다.
    /// 기록 · 다시 보기에 쓰일 수 있으므로 있는 값을 바꾸지 말고 뒤에 더한다.
    /// 세 키 모두 Source = 행동한 쪽, Target = 당한 쪽이다.
    /// </summary>
    public enum BattleEventKind
    {
        /// <summary>표적이 바뀜(intent). Source = 고른 유닛, Target = 새 표적(없으면 NoId), Value = 0, Position = 고른 유닛 자리.</summary>
        Intent = 0,
        /// <summary>기본 공격이 맞음(hit). Source = 공격자, Target = 맞은 유닛, Value = 실제로 깎인 HP(같은 틱에 먼저 0 이 됐으면 0), Position = 맞은 유닛 자리.</summary>
        Hit = 1,
        /// <summary>HP 0 → 전투 불능(down · 사망 아님). Source = 마지막 HP 를 깎은 유닛, Target = 쓰러진 유닛, Value = 0, Position = 쓰러진 자리. #3 쓰러짐 표현이 듣는다.</summary>
        Down = 2,
    }

    /// <summary>시뮬이 한 틱 동안 모았다가 틱이 끝난 뒤 내주는 가벼운 이벤트 {키, 틱, 주체, 대상, 값, 좌표}. 값 형식이라 만들어도 힙을 쓰지 않는다.</summary>
    public readonly struct BattleEvent
    {
        public BattleEventKind Kind { get; }
        /// <summary>일어난 틱 번호(1부터).</summary>
        public int Tick { get; }
        public int SourceId { get; }
        public int TargetId { get; }
        public int Value { get; }
        public Vector2 Position { get; }

        public BattleEvent(BattleEventKind kind, int tick, int sourceId, int targetId, int value, Vector2 position)
        {
            Kind = kind;
            Tick = tick;
            SourceId = sourceId;
            TargetId = targetId;
            Value = value;
            Position = position;
        }

        public override string ToString()
        {
            return $"[tick {Tick}] {Kind} {SourceId}->{TargetId} value={Value} pos={Position}";
        }
    }
}
