using System.Collections.Generic;
using UnityEngine;

using Battles;
using Battles.Simulation;
using Characters;
using Commons;

namespace Managers
{
    public interface IBattleManager
    {
        // void BeginBattle<>
    }
    
    /// <summary>
    /// 전투의 유니티 쪽 진행자. 순수 전투 시뮬(BattleSimulation)과 캐릭터(표현)가 만나는 유일한 곳이다. 시뮬은 이 클래스를 모른다.
    /// 프레임마다(GameManager.Update — 입력 다음):
    ///   ① 캐릭터마다 이동 입력(ActionController.MoveInput)을 시뮬에 넣는다(몬스터는 늘 0 → 자동 행동만).
    ///   ② 흐른 시간만큼 고정 간격 틱을 돌린다(TickAccumulator). 틱이 하나 끝날 때마다 그 틱의 이벤트를 OnBattleEvent 로 알린다.
    ///   ③ 캐릭터 그림을 시뮬 상태에 맞춘다(BattleUnitPresenter · 보간 없음).
    /// 유닛 번호 = _characters 의 칸 번호(CharacterManager.AllCharacters 가운데 켜져 있는 것의 순서).
    /// </summary>
    public class BattleManager : Entity
    {
        // 한 프레임에 따라잡는 최대 시간(초 · 60틱/초면 15틱). 이보다 밀린 시간은 버린다(멈칫 · 디버거 정지 뒤 몰아서 돌지 않게).
        // 틱 수가 아니라 시간으로 정한다 — 틱 수/초를 올려도 평소 프레임에서 한도에 걸려 전투가 느려지지 않게.
        private const float MaxCatchUpSeconds = 0.25f;

        /// <summary>
        /// 틱이 하나 끝날 때마다 그 틱의 이벤트를 생긴 순서대로 알린다(틱 도중에는 알리지 않는다).
        /// #3 쓰러짐 표현 · UI 가 여기에 붙고(Down: Source = 마지막으로 때린 유닛, Target = 쓰러진 유닛), TryGetCharacter 로 캐릭터를 찾는다.
        /// 받는 쪽은 읽기만 한다. 받는 쪽에서 ChainUpdate 를 다시 불러도 무시된다.
        /// </summary>
        public event System.Action<BattleEvent> OnBattleEvent = null;

        private readonly List<Character> _characters = new List<Character>();                    // [유닛 번호]
        private readonly List<BattleUnitPresenter> _presenters = new List<BattleUnitPresenter>(); // [유닛 번호]

        private BattleSimulation _simulation = null;
        private TickAccumulator _tickAccumulator = null;
        private bool _logEvents = false;
        private bool _isRunningTicks = false;

        public void Initialize(CharacterManager characterManager, BattleRules rules, bool logEvents)
        {
            _characters.Clear();
            _presenters.Clear();
            _logEvents = logEvents;

            _simulation = new BattleSimulation(rules, new NearestEnemySelector());
            int maxTicksPerFrame = Mathf.Max(1, Mathf.CeilToInt(MaxCatchUpSeconds * _simulation.TicksPerSecond));
            _tickAccumulator = new TickAccumulator(_simulation.TicksPerSecond, maxTicksPerFrame);

            RegisterCharacters(characterManager);

            Debug.Log($"[BattleManager] 전투 유닛 {_characters.Count}명 등록");
        }
        
        public override void Activate()
        {
            _tickAccumulator?.Reset();

            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();

            // 이벤트를 받은 쪽이 다시 부르면(재진입) 무시한다 — 알리는 도중에 시뮬이 다음 틱을 돌아 이벤트 목록이 비워지지 않게.
            if (!IsActive || _simulation == null || _tickAccumulator == null || _isRunningTicks)
                return;

            _isRunningTicks = true;
            try
            {
                PushMoveInputs();

                int tickCount = _tickAccumulator.Advance(Time.deltaTime);
                for (int i = 0; i < tickCount; i++)
                {
                    _simulation.Tick();
                    ObserveTick();
                    DispatchTickEvents();
                }

                PresentUnits();
            }
            finally
            {
                _isRunningTicks = false;
            }
        }

        /// <summary>유닛 번호의 캐릭터. 없거나 파괴됐으면 false.</summary>
        public bool TryGetCharacter(int unitId, out Character character)
        {
            character = (uint)unitId < (uint)_characters.Count ? _characters[unitId] : null;
            return character != null;
        }

        private void RegisterCharacters(CharacterManager characterManager)
        {
            var characters = characterManager?.AllCharacters;
            if (characters == null)
                return;

            for (int i = 0; i < characters.Count; i++)
            {
                var character = characters[i];

                // 꺼진 캐릭터는 전투에 넣지 않는다(보이지 않는 채 싸우거나 표적이 되지 않게).
                if (character == null || !character.gameObject.activeInHierarchy)
                    continue;

                // 편은 여기 한 곳에서 정한다: 플레이어 캐릭터 = 아군, 그 밖(몬스터) = 적.
                var team = character is PlayerCharacter ? BattleTeam.Ally : BattleTeam.Enemy;
                var stats = (character.BattleStats ?? new BattleStatsSettings()).ToBattleUnitStats();
                Vector3 position = character.transform.position;

                int unitId = _simulation.AddUnit(team, new Vector2(position.x, position.y), stats);
                _characters.Add(character);
                _presenters.Add(new BattleUnitPresenter(character));

                Debug.Assert(unitId == _characters.Count - 1, "[BattleManager] 유닛 번호와 캐릭터 칸 번호가 어긋났습니다.");
            }
        }

        // 조작 의도를 시뮬에 넘긴다. 몬스터는 아무도 입력을 주지 않아 늘 0 이다.
        private void PushMoveInputs()
        {
            for (int unitId = 0; unitId < _characters.Count; unitId++)
            {
                var character = _characters[unitId];
                var actionController = character != null ? character.ActionController : null;

                _simulation.SetMoveInput(unitId, actionController != null ? actionController.MoveInput : Vector2.zero);
            }
        }

        // 방금 끝난 한 틱의 이벤트를 생긴 순서대로 알린다.
        private void DispatchTickEvents()
        {
            int eventCount = _simulation.EventCount;
            for (int i = 0; i < eventCount; i++)
            {
                var battleEvent = _simulation.GetEvent(i);

                if (_logEvents)
                    LogEvent(battleEvent);

                OnBattleEvent?.Invoke(battleEvent);
            }
        }

        // 방금 끝난 한 틱의 상태를 표현에 알린다(그림은 바꾸지 않는다 — 한 프레임에 틱이 여럿 돌아도 틱 단위로 이어진 상태를 세게).
        private void ObserveTick()
        {
            for (int unitId = 0; unitId < _presenters.Count; unitId++)
            {
                if (_simulation.TryGetUnit(unitId, out var unit))
                    _presenters[unitId].ObserveTick(unit);
            }
        }

        private void PresentUnits()
        {
            for (int unitId = 0; unitId < _presenters.Count; unitId++)
            {
                if (_simulation.TryGetUnit(unitId, out var unit))
                    _presenters[unitId].Present(unit);
            }
        }

        // HP UI 가 생기기 전까지 확인용(GameManager 의 Log Battle Events). 이벤트마다 문자열을 만든다.
        private void LogEvent(BattleEvent battleEvent)
        {
            switch (battleEvent.Kind)
            {
                case BattleEventKind.Hit:
                    _simulation.TryGetUnit(battleEvent.TargetId, out var target);
                    Debug.Log($"[Battle] {battleEvent.Tick}틱 {GetCharacterName(battleEvent.SourceId)} → {GetCharacterName(battleEvent.TargetId)} 피해 {battleEvent.Value} (HP {target?.Hp ?? 0}/{target?.MaxHp ?? 0})");
                    break;

                case BattleEventKind.Down:
                    Debug.Log($"[Battle] {battleEvent.Tick}틱 {GetCharacterName(battleEvent.TargetId)} 전투 불능 (마지막 타격: {GetCharacterName(battleEvent.SourceId)})");
                    break;
            }
        }

        private string GetCharacterName(int unitId)
        {
            return TryGetCharacter(unitId, out var character) ? character.name : $"#{unitId}";
        }
    }
}
