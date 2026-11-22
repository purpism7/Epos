using System.Collections.Generic;
using UnityEngine;

namespace Battles.Simulation
{
    /// <summary>
    /// 전투 시뮬레이션의 순수한 중심. 표현(그림 · 스파인 · Transform · 시간 · 씬)을 모르고, 고정 간격 한 걸음(틱)씩만 나아간다.
    /// - 유니티에서 쓰는 것은 Vector2 · Mathf 뿐이다(UnityEngine.CoreModule 만으로 컴파일된다).
    /// - 같은 규칙 · 같은 등록(편 · 자리 · 능력치, 순서까지) · 같은 틱별 이동 입력 → 같은 결과(같은 빌드 · 플랫폼).
    ///   지금은 난수를 쓰는 규칙이 없어 시드를 받지 않는다. 처음 난수가 필요해지면 시드 난수 하나를 생성자로 주입한다.
    /// - 유닛 번호 = 등록 순서(0, 1, 2 …). 유닛은 지우지 않는다(HP 0 = 전투 불능). 모든 순회는 번호 오름차순.
    /// - 이벤트는 틱 안에서 내보내지 않고 모아 두었다가, Tick() 이 끝난 뒤 EventCount · GetEvent 로 읽어 간다(다음 Tick() 이 비운다).
    ///
    /// 한 틱의 순서(바꾸지 않는다):
    ///   ① 판단 — 끝난 휘두르기를 끝내고 표적을 고른다. 틱 시작 때의 상태만 읽는다.
    ///   ② 계획 — 이동 · 추격 · 휘두르기 시작을 정한다. 틱 시작 때의 위치를 읽는다.
    ///   ③ 이동 — ②에서 정한 만큼 한꺼번에 옮긴다(앞 번호가 옮긴 자리를 뒷 번호가 보지 않는다 → 결과가 등록 순서에 기대지 않는다).
    ///   ④ 타격 — 맞는 때가 된 휘두르기의 피해. 전투 불능 표시는 ⑤에서 하므로 같은 틱의 타격은 동시다.
    ///   ⑤ 불능 — HP 0 이 된 유닛을 전투 불능으로 표시하고 알린다.
    /// </summary>
    public sealed class BattleSimulation
    {
        // 틱 수/초 상한(초 → 틱 변환이 int 를 넘치지 않게).
        private const int MaxTicksPerSecond = 1000;

        // 쫓아가기는 사거리보다 이만큼(표적이 나를 쫓아오는 중이 아니면 + 표적의 1틱 이동 거리) 안쪽까지 다가가 멈춘다
        // (실수 오차로 사거리 바로 바깥에 멈춰 휘두르지 못하는 일 · 서 있는 표적 앞 경계에서 쫓기와 서기가 번갈아 떨리는 일을 막는다).
        // 움직이는 표적 앞에서는 쫓기 · 서기가 틱마다 번갈아 나올 수 있다 — 그림이 떨리지 않게 하는 것은 표현(BattleUnitPresenter)의 몫이다.
        private const float ChaseStopInset = 0.05f;

        // 한 틱의 이벤트는 보통 이보다 훨씬 적다. 넘으면 목록이 한 번 늘어난 뒤 그 크기를 계속 쓴다.
        private const int InitialEventCapacity = 64;

        private readonly List<BattleUnit> _units = new List<BattleUnit>();
        private readonly List<BattleEvent> _events = new List<BattleEvent>(InitialEventCapacity);
        private readonly ITargetSelector _targetSelector = null;
        private readonly int _decisionTicks = 1;

        public int TicksPerSecond { get; }

        /// <summary>마지막으로 끝난 틱 번호. 아직 돌지 않았으면 0, 첫 Tick() 이 1번 틱이다.</summary>
        public int CurrentTick { get; private set; } = 0;

        /// <summary>모든 유닛(번호 오름차순 = 칸 번호). 전투 불능 유닛도 빠지지 않는다.</summary>
        public IReadOnlyList<BattleUnit> Units => _units;
        public int UnitCount => _units.Count;

        /// <summary>마지막 Tick() 에서 생긴 이벤트 수. GetEvent(0 ~ EventCount-1) 로 생긴 순서대로 읽는다.</summary>
        public int EventCount => _events.Count;

        /// <param name="rules">전투 규칙. 잘못된 값은 고쳐 쓴다(틱 수/초 1 ~ 1000, 판단 주기 최소 1틱).</param>
        /// <param name="targetSelector">표적 고르기. null 이면 가장 가까운 적(NearestEnemySelector).</param>
        public BattleSimulation(BattleRules rules, ITargetSelector targetSelector)
        {
            TicksPerSecond = Mathf.Clamp(rules.TicksPerSecond, 1, MaxTicksPerSecond);
            _decisionTicks = Mathf.Max(1, BattleUnit.SecondsToTicks(rules.DecisionSec, TicksPerSecond));
            _targetSelector = targetSelector ?? new NearestEnemySelector();
        }

        /// <summary>
        /// 유닛을 넣고 번호를 돌려준다(0부터 넣은 순서 = Units 의 칸 번호). 능력치는 이때 한 번 읽어 틱 단위로 고정한다.
        /// 다음 틱에 바로 표적을 고른다. 위치가 NaN · 무한대면 (0, 0).
        /// </summary>
        public int AddUnit(BattleTeam team, Vector2 position, BattleUnitStats stats)
        {
            int unitId = _units.Count;
            var safePosition = IsFinite(position) ? position : Vector2.zero;

            _units.Add(new BattleUnit(unitId, team, safePosition, stats, TicksPerSecond, CurrentTick));
            return unitId;
        }

        public bool TryGetUnit(int unitId, out BattleUnit unit)
        {
            if ((uint)unitId < (uint)_units.Count)
            {
                unit = _units[unitId];
                return true;
            }

            unit = null;
            return false;
        }

        /// <summary>마지막 Tick() 의 index 번째 이벤트(생긴 순서).</summary>
        public BattleEvent GetEvent(int index)
        {
            return _events[index];
        }

        /// <summary>
        /// 이동 입력(키 축). 바꿀 때까지 유지된다. 0 이 아니면 그 유닛은 공격하지 않고 이 쪽으로 움직이고, 0 이면 자동 기본 공격으로 돌아간다.
        /// NaN · 무한대 · 길이 0.01 미만은 0, 길이가 1 을 넘으면 1 로 줄인다. 없는 번호는 무시한다. 전투 불능 유닛은 입력을 받아도 움직이지 않는다.
        /// </summary>
        public void SetMoveInput(int unitId, Vector2 moveAxis)
        {
            if (TryGetUnit(unitId, out var unit))
                unit.MoveInput = SanitizeMoveInput(moveAxis);
        }

        /// <summary>한 틱 나아간다. 이 틱의 이벤트는 돌아온 뒤 EventCount · GetEvent 로 읽는다.</summary>
        public void Tick()
        {
            _events.Clear();
            CurrentTick++;

            int now = CurrentTick;
            int count = _units.Count;

            for (int i = 0; i < count; i++)
                DecideTarget(_units[i], now);

            for (int i = 0; i < count; i++)
                PlanAction(_units[i], now);

            for (int i = 0; i < count; i++)
                ApplyMovement(_units[i]);

            for (int i = 0; i < count; i++)
                ResolveHit(_units[i], now);

            for (int i = 0; i < count; i++)
                ResolveDown(_units[i], now);
        }

        // ① 판단: 판단 차례(주기 · 표적 잃음)에만 표적을 고른다. 자기 휘두르기 · 표적 · 판단 틱만 바꾼다.
        private void DecideTarget(BattleUnit unit, int now)
        {
            if (unit.IsDown)
                return;

            // 동작 길이가 다 지난 휘두르기는 여기서 끝낸다 → 연달아 휘두를 때(간격 = 동작 길이)도 판단 차례를 놓치지 않는다.
            EndSwingIfFinished(unit, now);

            // 이동 입력이 있으면 공격하지 않는다(움직이면 멈춤). 손을 뗀 틱에 바로 다시 고르도록 판단 차례를 지금으로 당겨 둔다.
            if (unit.HasMoveInput)
            {
                CancelSwing(unit);
                ChangeTarget(unit, BattleUnit.NoId, now);
                unit.DecideAtTick = now;
                return;
            }

            if (!unit.CanAttack)
            {
                ChangeTarget(unit, BattleUnit.NoId, now);
                return;
            }

            bool targetLost = unit.TargetId != BattleUnit.NoId && !IsValidTarget(unit, unit.TargetId);

            // 휘두르는 중에는 표적을 바꾸지 않는다(표적을 잃었을 때만 끊는다).
            if (unit.IsSwinging && !targetLost)
                return;

            // 다시 고르는 틱 사이에는 기억해 둔 표적을 믿는다.
            if (!targetLost && now < unit.DecideAtTick)
                return;

            // 표적을 잃은 휘두르기는 끊는다(피해 전이면 간격을 돌려받는다).
            CancelSwing(unit);

            int selectedId = _targetSelector.SelectTarget(unit, _units);
            if (!IsValidTarget(unit, selectedId))
                selectedId = BattleUnit.NoId;   // 바꿔 끼운 고르기가 잘못 골라도 안전하게.

            unit.DecideAtTick = now + _decisionTicks;
            ChangeTarget(unit, selectedId, now);
        }

        // ② 계획: 이번 틱에 할 일을 정한다. 위치는 틱 시작 때 값을 읽고, 옮기는 것은 ③에서 한꺼번에 한다.
        private void PlanAction(BattleUnit unit, int now)
        {
            unit.PlannedStep = Vector2.zero;

            if (unit.IsDown)
                return;

            // 이동 입력이 이긴다: 직접 움직이고 휘두르지 않는다.
            if (unit.HasMoveInput)
            {
                unit.PlannedStep = unit.MoveInput * unit.MoveSpeedPerTick;
                unit.LookToward(unit.MoveInput);
                unit.Activity = BattleUnitActivity.Moving;
                return;
            }

            bool hasTarget = TryGetValidTarget(unit, out var target);

            // 휘두르는 중이면 서서 표적을 본다(표적을 잃은 휘두르기는 ①에서 끊겼다).
            if (unit.IsSwinging)
            {
                if (hasTarget)
                    unit.LookToward(target.Position - unit.Position);

                unit.Activity = BattleUnitActivity.Attacking;
                return;
            }

            if (!hasTarget)
            {
                unit.Activity = BattleUnitActivity.Idle;
                return;
            }

            Vector2 toTarget = target.Position - unit.Position;
            float distance = toTarget.magnitude;
            unit.LookToward(toTarget);

            if (distance <= unit.AttackRange)
            {
                if (now >= unit.NextAttackTick)
                    StartSwing(unit, now);
                else
                    unit.Activity = BattleUnitActivity.Idle;   // 사거리 안에서 다음 휘두르기를 기다린다(표적을 본다).

                return;
            }

            if (unit.MoveSpeedPerTick <= 0f)
            {
                unit.Activity = BattleUnitActivity.Idle;       // 움직일 수 없으면 제자리에서 표적을 보며 기다린다.
                return;
            }

            // 사거리보다 ChaseStopInset 만큼 안쪽까지 다가가되, 표적이 이번 틱에 움직일 수 있는 거리(표적의 고정 능력치)만큼 더 들어간다.
            // 표적은 ③에서 같이 움직이므로, 이것이 없으면 0.05/틱보다 빨리 달아나는 표적을 더 빠른 쪽이 영원히 사거리 바로 바깥에서 쫓기만 한다.
            // 서 있는 표적에게는 그만큼 더 깊이 설 뿐이다.
            // 표적이 나를 쫓아오는 중이면(나를 겨눔 · 이동 입력 없음) 더하지 않는다 — 둘이 서로 더 들어가면 빠르고 사거리가 짧을 때
            // 매 틱 서로의 자리로 건너뛰기만 하고 영원히 휘두르지 못한다. ①에서 모두의 표적 · 입력이 정해진 뒤라 등록 순서에 기대지 않는다.
            // 표적이 틱 시작 때 있던 자리는 넘어가지 않는다(사거리가 아주 짧을 때). 그래서 한 틱에 사거리보다 멀리 달아나는 표적
            // (속도 > 사거리 × 틱 수/초, 예: 사거리 0.1 · 6 이상/초)은 잡지 못한다 ⚑.
            // distance > AttackRange >= 0.1 이라 나눗셈은 안전하다.
            bool isTargetClosingIn = target.TargetId == unit.Id && !target.HasMoveInput;
            float targetLead = isTargetClosingIn ? 0f : target.MoveSpeedPerTick;
            float stepLength = Mathf.Min(unit.MoveSpeedPerTick, distance - unit.AttackRange + ChaseStopInset + targetLead);
            stepLength = Mathf.Min(stepLength, distance);
            unit.PlannedStep = toTarget * (stepLength / distance);
            unit.Activity = BattleUnitActivity.Chasing;
        }

        // ③ 이동: 모두 한꺼번에 옮긴다.
        private static void ApplyMovement(BattleUnit unit)
        {
            unit.Position += unit.PlannedStep;
            unit.PlannedStep = Vector2.zero;
        }

        // ④ 타격: 맞는 때가 된 휘두르기의 피해. 불능 표시는 ⑤에서 하므로 이 틱에 먼저 0 이 된 표적도 맞는다(동시 타격).
        // 휘두르기가 시작되면 맞는 때에 거리를 다시 재지 않는다(휘두른 공격은 들어간다 ⚑).
        private void ResolveHit(BattleUnit unit, int now)
        {
            if (!unit.IsSwinging || unit.HasHitThisSwing)
                return;

            if (now < unit.SwingStartTick + unit.AttackHitTicks)
                return;

            unit.HasHitThisSwing = true;

            // 휘두르는 동안 표적은 바뀌지 않고 표적을 잃은 휘두르기는 ①에서 끊기므로 보통 늘 겨눌 수 있다(지키는 검사).
            if (!TryGetValidTarget(unit, out var target))
                return;

            int removedHp = Mathf.Min(unit.AttackDamage, target.Hp);
            target.Hp -= removedHp;
            if (removedHp > 0)
                target.LastAttackerId = unit.Id;

            _events.Add(new BattleEvent(BattleEventKind.Hit, now, unit.Id, target.Id, removedHp, target.Position));
        }

        // ⑤ 불능: HP 0 이면 전투 불능. 지우지 않고 자리에 남긴다.
        private void ResolveDown(BattleUnit unit, int now)
        {
            if (unit.IsDown || unit.Hp > 0)
                return;

            unit.IsDown = true;
            unit.IsSwinging = false;
            unit.TargetId = BattleUnit.NoId;
            unit.Activity = BattleUnitActivity.Down;

            _events.Add(new BattleEvent(BattleEventKind.Down, now, unit.LastAttackerId, unit.Id, 0, unit.Position));
        }

        private static void EndSwingIfFinished(BattleUnit unit, int now)
        {
            if (unit.IsSwinging && now >= unit.SwingStartTick + unit.AttackDurationTicks)
                unit.IsSwinging = false;
        }

        private static void StartSwing(BattleUnit unit, int now)
        {
            unit.IsSwinging = true;
            unit.SwingStartTick = now;
            unit.HasHitThisSwing = false;
            unit.NextAttackTick = now + unit.AttackIntervalTicks;
            unit.AttackSerial++;
            unit.Activity = BattleUnitActivity.Attacking;
        }

        // 휘두르기를 끊는다. 피해가 들어가기 전이면 간격을 돌려준다(헛손질로 다음 휘두르기가 늦어지지 않게).
        // 피해가 들어간 뒤면 간격은 그대로다(움직였다 멈추기로 공격 간격을 줄일 수 없게).
        private static void CancelSwing(BattleUnit unit)
        {
            if (!unit.IsSwinging)
                return;

            unit.IsSwinging = false;

            if (!unit.HasHitThisSwing)
                unit.NextAttackTick = unit.SwingStartTick;
        }

        private void ChangeTarget(BattleUnit unit, int targetId, int now)
        {
            if (unit.TargetId == targetId)
                return;

            unit.TargetId = targetId;
            _events.Add(new BattleEvent(BattleEventKind.Intent, now, unit.Id, targetId, 0, unit.Position));
        }

        private bool TryGetValidTarget(BattleUnit unit, out BattleUnit target)
        {
            if (TryGetUnit(unit.TargetId, out target) && unit.CanTarget(target))
                return true;

            target = null;
            return false;
        }

        private bool IsValidTarget(BattleUnit unit, int targetId)
        {
            return TryGetUnit(targetId, out var target) && unit.CanTarget(target);
        }

        private static Vector2 SanitizeMoveInput(Vector2 moveAxis)
        {
            float sqrLength = moveAxis.x * moveAxis.x + moveAxis.y * moveAxis.y;

            // 0 · NaN 은 비교가 거짓이라 함께 걸러진다. 무한대도 입력 없음으로 본다.
            if (!(sqrLength > BattleUnit.MinDirectionSqrLength) || float.IsInfinity(sqrLength))
                return Vector2.zero;

            return sqrLength > 1f ? moveAxis / Mathf.Sqrt(sqrLength) : moveAxis;
        }

        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y);
        }
    }
}
