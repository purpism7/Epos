using UnityEngine;

namespace Battles.Simulation
{
    /// <summary>편. 다른 편끼리만 서로 겨눈다.</summary>
    public enum BattleTeam
    {
        Ally = 0,
        Enemy = 1,
    }

    /// <summary>유닛이 이번 틱에 하는 일. 표현(BattleUnitPresenter)은 이것을 보고 그림(Idle · Run · Skill01)을 고른다.</summary>
    public enum BattleUnitActivity
    {
        /// <summary>서 있음 — 표적 없음 · 사거리 안에서 다음 휘두르기를 기다림 · 쫓아갈 수 없음(속도 0).</summary>
        Idle = 0,
        /// <summary>이동 입력으로 직접 움직임(공격하지 않는다).</summary>
        Moving = 1,
        /// <summary>표적에게 사거리까지 다가감.</summary>
        Chasing = 2,
        /// <summary>휘두르는 중(서서 휘두름). 이동 입력만 끊을 수 있다.</summary>
        Attacking = 3,
        /// <summary>전투 불능(HP 0 · 사망 아님). 자리에 남지만 아무것도 하지 않고 표적이 되지 않는다. 되돌릴 수 없다.</summary>
        Down = 4,
    }

    /// <summary>
    /// 시뮬 유닛 하나의 값과 상태. 바깥(표현 · 시험)에는 읽기만 열고, 바꾸는 것은 BattleSimulation 뿐이다(internal).
    /// 능력치는 등록할 때 정리하고 틱 단위로 바꿔 고정한다(그 뒤 인스펙터 값을 다시 읽지 않는다).
    /// </summary>
    public sealed class BattleUnit
    {
        public const int NoId = -1;

        // 이보다 짧은(길이 0.01 미만) 방향 · 이동 입력은 '없음'으로 본다. Facing 의 문턱과 같다.
        internal const float MinDirectionSqrLength = 0.0001f;

        private const float MinAttackRange = 0.1f;
        // 초 → 틱 변환 상한(1시간). 잘못 넣은 큰 값이 int 를 넘치지 않게.
        private const float MaxSeconds = 3600f;

        // ── 고정 값(등록 때 정해짐) ──
        public int Id { get; }
        public BattleTeam Team { get; }
        public int MaxHp { get; }
        public int AttackDamage { get; }
        public float AttackRange { get; }
        public float MoveSpeedPerTick { get; }
        public int AttackHitTicks { get; }
        public int AttackDurationTicks { get; }
        public int AttackIntervalTicks { get; }

        /// <summary>공격력이 0 이면 표적을 고르지도 공격하지도 않는다(허수아비).</summary>
        public bool CanAttack => AttackDamage > 0;

        // ── 상태(바깥에서는 읽기만) ──
        public Vector2 Position { get; internal set; }
        public int Hp { get; internal set; }
        public bool IsDown { get; internal set; }

        /// <summary>지금 표적 번호. 없으면 NoId.</summary>
        public int TargetId { get; internal set; } = NoId;
        public BattleUnitActivity Activity { get; internal set; } = BattleUnitActivity.Idle;

        /// <summary>표현이 바라볼 쪽(이동 축 · 표적 쪽, 크기 무관). 판정에는 쓰지 않는다. 처음엔 (0, 0) = 방향 없음.</summary>
        public Vector2 LookDirection { get; private set; } = Vector2.zero;

        /// <summary>휘두르기를 시작할 때마다 1 늘어난다. 표현이 연달아 시작된 휘두르기를 놓치지 않고 처음부터 다시 튼다.</summary>
        public int AttackSerial { get; internal set; }

        /// <summary>이동 입력(정리된 값). 0 이 아니면 공격하지 않고 이 쪽으로 움직인다.</summary>
        public Vector2 MoveInput { get; internal set; } = Vector2.zero;
        public bool HasMoveInput => MoveInput.sqrMagnitude > 0f;

        // ── 진행 값(BattleSimulation 전용 · 시간은 틱) ──
        internal int DecideAtTick { get; set; }
        internal bool IsSwinging { get; set; }
        internal int SwingStartTick { get; set; }
        internal bool HasHitThisSwing { get; set; }
        internal int NextAttackTick { get; set; }
        internal int LastAttackerId { get; set; } = NoId;
        internal Vector2 PlannedStep { get; set; } = Vector2.zero;

        internal BattleUnit(int id, BattleTeam team, Vector2 position, BattleUnitStats stats, int ticksPerSecond, int currentTick)
        {
            ticksPerSecond = Mathf.Max(1, ticksPerSecond);

            Id = id;
            Team = team;
            Position = position;

            MaxHp = Mathf.Max(1, stats.MaxHp);
            Hp = MaxHp;
            AttackDamage = Mathf.Max(0, stats.AttackDamage);
            AttackRange = Mathf.Max(MinAttackRange, FiniteNonNegative(stats.AttackRange));
            MoveSpeedPerTick = FiniteNonNegative(stats.MoveSpeed) / ticksPerSecond;

            // 휘두르기는 최소 1틱, 피해는 휘두르기가 끝나기 전, 간격은 휘두르기보다 짧지 않다.
            AttackDurationTicks = Mathf.Max(1, SecondsToTicks(stats.AttackDurationSec, ticksPerSecond));
            AttackHitTicks = Mathf.Clamp(SecondsToTicks(stats.AttackHitSec, ticksPerSecond), 0, AttackDurationTicks - 1);
            AttackIntervalTicks = Mathf.Max(AttackDurationTicks, SecondsToTicks(stats.AttackIntervalSec, ticksPerSecond));

            // 등록한 다음 틱에 바로 표적을 고르고, 바로 휘두를 수 있다.
            DecideAtTick = currentTick;
            NextAttackTick = currentTick;
        }

        /// <summary>이 유닛이 other 를 겨눌 수 있는가: 자기가 아니고 · 다른 편이고 · 전투 불능이 아니다.</summary>
        public bool CanTarget(BattleUnit other)
        {
            return other != null && other != this && other.Team != Team && !other.IsDown;
        }

        internal void LookToward(Vector2 direction)
        {
            if (direction.sqrMagnitude > MinDirectionSqrLength)
                LookDirection = direction;
        }

        /// <summary>초 → 틱(반올림). 0 · 음수 · NaN 은 0, 너무 큰 값(무한대 포함)은 MaxSeconds 로 자른다.</summary>
        internal static int SecondsToTicks(float seconds, int ticksPerSecond)
        {
            if (!(seconds > 0f))
                return 0;

            return Mathf.RoundToInt(Mathf.Min(seconds, MaxSeconds) * ticksPerSecond);
        }

        // 음수 · NaN · 무한대는 0.
        private static float FiniteNonNegative(float value)
        {
            return value > 0f && !float.IsInfinity(value) ? value : 0f;
        }
    }
}
