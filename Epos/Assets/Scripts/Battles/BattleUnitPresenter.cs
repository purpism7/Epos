using UnityEngine;

using Battles.Simulation;
using Characters;

namespace Battles
{
    /// <summary>
    /// 시뮬 유닛 하나를 캐릭터 그림에 옮기는 얇은 표현 어댑터. 시뮬을 읽기만 하고 바꾸지 않는다.
    /// - 프레임마다: 위치(시뮬 x, y · z 는 그대로) · 그리기 순서(y) · 바라보는 쪽(CharacterAnimator.Face).
    /// - 행동 그림(Idle · Run · Skill01)은 하는 일이 바뀔 때만 ActionController 에 알린다.
    ///   공격은 새 휘두르기가 시작될 때마다(AttackSerial 이 바뀔 때) 처음부터 다시 튼다.
    /// - 한 프레임에 틱이 여럿 돌아도 마지막 상태만 보여 준다(보간 없음).
    /// - 쫓기 → 서기는 서기가 ChaseToIdleHoldTicks 틱 내리 이어진 뒤에야 바꾼다(움직이는 표적 앞 사거리 경계에서 시뮬이 쫓기 · 서기를
    ///   틱마다 번갈아 내도 달리기 · 대기 그림이 프레임마다 처음부터 다시 틀리지 않게). 이어진 틱 수는 BattleManager 가 틱마다 부르는
    ///   ObserveTick 이 센다(한 프레임에 틱이 여럿 돌아도 틱 단위로 센다). 시뮬 규칙은 건드리지 않는다.
    /// </summary>
    public sealed class BattleUnitPresenter
    {
        // 쫓다가 선 뒤 대기 그림으로 바꾸기까지 서기가 내리 이어져야 하는 틱 수(⚑ 6 = 0.1초 · 60틱/초). 그사이 다시 쫓으면 달리기를 끊지 않고 잇는다.
        private const int ChaseToIdleHoldTicks = 6;

        private readonly Character _character = null;

        // 마지막으로 그림에 반영한 상태. 같으면 다시 알리지 않는다.
        private bool _hasShownActivity = false;
        private BattleUnitActivity _shownActivity = BattleUnitActivity.Idle;
        private int _shownAttackSerial = 0;

        // 시뮬이 서기(Idle)를 내리 이어 온 틱 수(ChaseToIdleHoldTicks 에서 멈춘다). ObserveTick 이 틱마다 센다.
        private int _consecutiveIdleTicks = 0;

        // 지난 Present 뒤로 쫓은 틱이 있었는가(그 뒤 서기만 이어졌을 때만 유지). 프레임에 틱이 여럿 돌 때 가운데 틱의 쫓기를 놓치지 않게.
        private bool _chasedSincePresent = false;

        public BattleUnitPresenter(Character character)
        {
            _character = character;
        }

        /// <summary>시뮬 틱이 하나 끝날 때마다 부른다(그림은 바꾸지 않고 틱 단위 상태만 센다).</summary>
        public void ObserveTick(BattleUnit unit)
        {
            if (unit == null)
                return;

            _consecutiveIdleTicks = unit.Activity == BattleUnitActivity.Idle
                ? Mathf.Min(_consecutiveIdleTicks + 1, ChaseToIdleHoldTicks)
                : 0;

            // 이번 프레임에 쫓은 틱이 있었는지(뒤에 서기만 이어지면 유지 · 다른 일을 하면 지운다).
            if (unit.Activity == BattleUnitActivity.Chasing)
                _chasedSincePresent = true;
            else if (unit.Activity != BattleUnitActivity.Idle)
                _chasedSincePresent = false;
        }

        /// <summary>프레임마다 마지막 틱의 상태를 그림에 옮긴다.</summary>
        public void Present(BattleUnit unit)
        {
            // 캐릭터가 파괴됐으면(유니티 null) 아무것도 하지 않는다.
            if (unit == null || _character == null)
                return;

            var transform = _character.Transform;
            if (transform == null)
                return;

            Vector2 position = unit.Position;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            _character.SetSortingOrder(position.y);

            // 그림을 먼저 고른다(새 그림은 이 쪽을 보고 한 번만 튼다). 그 뒤 바뀐 방향만 반영한다(같으면 아무 일 없음).
            ShowActivity(unit);
            _character.CharacterAnimator?.Face(unit.LookDirection);

            _chasedSincePresent = false;
        }

        private void ShowActivity(BattleUnit unit)
        {
            var actionController = _character.ActionController;
            if (actionController == null)
                return;

            var activity = unit.Activity;

            // 한 프레임에 틱이 여럿 돌 때(예: 30fps = 2틱) 마지막 틱만 서기여도 그 프레임에 쫓았다면 쫓기로 본다
            // (그렇지 않으면 움직이는 동안 대기 그림이 보인다). 서기가 내리 ChaseToIdleHoldTicks 넘게 이어졌으면 서기 그대로.
            if (activity == BattleUnitActivity.Idle && _chasedSincePresent && _consecutiveIdleTicks < ChaseToIdleHoldTicks)
                activity = BattleUnitActivity.Chasing;

            bool isNewActivity = !_hasShownActivity || activity != _shownActivity;

            switch (activity)
            {
                case BattleUnitActivity.Attacking:
                    if (isNewActivity || unit.AttackSerial != _shownAttackSerial)
                        actionController.ShowAttack(unit.LookDirection);
                    break;

                case BattleUnitActivity.Moving:
                    if (isNewActivity)
                        actionController.ShowMove(unit.LookDirection);
                    break;

                case BattleUnitActivity.Chasing:
                    if (isNewActivity)
                        actionController.ShowChase(unit.LookDirection);
                    break;

                default:
                    // Idle · Down(쓰러지는 그림은 #3 에서 이 앞에 case 를 더한다 — 그때는 휘두르기 그림을 기다리지 않는다).
                    // 공격 그림이 아직 도는 중이면 바꾸지 않고 보여 준 것으로 적지도 않는다 → 다음 프레임에 다시 시도한다.
                    if (isNewActivity)
                    {
                        // 쫓다가 섰으면 서기가 내리 이어질 때까지 달리기를 둔다(그사이 다시 쫓으면 달리기를 그대로 잇는다).
                        if (activity == BattleUnitActivity.Idle && _hasShownActivity && _shownActivity == BattleUnitActivity.Chasing &&
                            _consecutiveIdleTicks < ChaseToIdleHoldTicks)
                            return;

                        if (!actionController.TryShowIdle())
                            return;
                    }
                    break;
            }

            _hasShownActivity = true;
            _shownActivity = activity;
            _shownAttackSerial = unit.AttackSerial;
        }
    }
}
