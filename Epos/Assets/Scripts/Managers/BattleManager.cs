using System.Collections.Generic;
using UnityEngine;

using Battles;
using Battles.Simulation;
using Characters;
using Commons;

namespace Managers
{
    /// <summary>판 흐름(BattleMode)이 보는 전투 진행자의 얼굴. 판의 결과를 알리고, 같은 배치로 다시 시작한다.</summary>
    public interface IBattleManager
    {
        /// <summary>지금 판의 결과. 끝나기 전에는 Ongoing.</summary>
        BattleOutcome Outcome { get; }

        /// <summary>한쪽이 모두 전투 불능이 된 틱이 끝난 뒤 한 번 알린다(그 틱의 OnBattleEvent 다음). 판이 끝나도 틱은 계속 돈다(남은 쪽은 서서 기다린다).</summary>
        event System.Action<BattleOutcome> OnBattleEnded;

        /// <summary>
        /// 같은 배치(처음 자리 · 편 · 순서)로 판을 처음부터 다시 시작한다. 능력치는 이때 캐릭터 인스펙터에서 다시 읽는다(판 사이에 값을 고쳐 볼 수 있게).
        /// 틱을 돌리는 도중(이벤트를 받는 쪽)에서 부르면 무시한다.
        /// </summary>
        void Restart();
    }
    
    /// <summary>
    /// 전투의 유니티 쪽 진행자. 순수 전투 시뮬(BattleSimulation)과 캐릭터(표현)가 만나는 유일한 곳이다. 시뮬은 이 클래스를 모른다.
    /// 프레임마다(GameManager.Update — 입력 다음):
    ///   ① 캐릭터마다 이동 입력(ActionController.MoveInput)을 시뮬에 넣는다(몬스터는 늘 0 → 자동 행동만).
    ///   ② 흐른 시간만큼 고정 간격 틱을 돌린다(TickAccumulator). 틱이 하나 끝날 때마다 그 틱의 이벤트를 OnBattleEvent 로 알린다.
    ///   ③ 캐릭터 그림을 시뮬 상태에 맞춘다(BattleUnitPresenter · 보간 없음).
    /// 유닛 번호 = _characters 의 칸 번호(CharacterManager.AllCharacters 가운데 켜져 있는 것의 순서).
    /// </summary>
    public class BattleManager : Entity, IBattleManager
    {
        // 한 프레임에 따라잡는 최대 시간(초 · 60틱/초면 15틱). 이보다 밀린 시간은 버린다(멈칫 · 디버거 정지 뒤 몰아서 돌지 않게).
        // 틱 수가 아니라 시간으로 정한다 — 틱 수/초를 올려도 평소 프레임에서 한도에 걸려 전투가 느려지지 않게.
        private const float MaxCatchUpSeconds = 0.25f;

        /// <summary>
        /// 틱이 하나 끝날 때마다 그 틱의 이벤트를 생긴 순서대로 알린다(틱 도중에는 알리지 않는다).
        /// 연출 · 소리 · UI 가 여기에 붙고(Down: Source = 마지막으로 때린 유닛, Target = 쓰러진 유닛), TryGetCharacter 로 캐릭터를 찾는다.
        /// (쓰러짐 그림 · 머리 위 HP 바는 이벤트가 아니라 시뮬 상태를 읽어 BattleUnitPresenter 가 맞춘다.)
        /// 받는 쪽은 읽기만 한다. 받는 쪽에서 ChainUpdate 를 다시 불러도 무시된다.
        /// </summary>
        public event System.Action<BattleEvent> OnBattleEvent = null;

        public event System.Action<BattleOutcome> OnBattleEnded = null;

        public BattleOutcome Outcome { get; private set; } = BattleOutcome.Ongoing;

        private readonly List<Character> _characters = new List<Character>();                    // [유닛 번호]
        private readonly List<BattleUnitPresenter> _presenters = new List<BattleUnitPresenter>(); // [유닛 번호]
        // 다시 하기가 같은 배치로 시작하도록 처음 등록할 때의 편 · 자리를 적어 둔다. [유닛 번호]
        private readonly List<BattleTeam> _teams = new List<BattleTeam>();
        private readonly List<Vector2> _startPositions = new List<Vector2>();

        private BattleRules _rules = default;
        private BattleSimulation _simulation = null;
        private TickAccumulator _tickAccumulator = null;
        private bool _logEvents = false;
        private bool _isRunningTicks = false;

        public void Initialize(CharacterManager characterManager, BattleRules rules, bool logEvents)
        {
            _characters.Clear();
            _presenters.Clear();
            _teams.Clear();
            _startPositions.Clear();
            _rules = rules;
            _logEvents = logEvents;

            CollectCharacters(characterManager);
            StartNewBattle();

            int maxTicksPerFrame = Mathf.Max(1, Mathf.CeilToInt(MaxCatchUpSeconds * _simulation.TicksPerSecond));
            _tickAccumulator = new TickAccumulator(_simulation.TicksPerSecond, maxTicksPerFrame);

            Debug.Log($"[BattleManager] 전투 유닛 {_characters.Count}명 등록");
        }

        public void Restart()
        {
            if (_simulation == null || _isRunningTicks)
                return;

            StartNewBattle();
            _tickAccumulator?.Reset();

            for (int unitId = 0; unitId < _presenters.Count; unitId++)
                _presenters[unitId].ResetForNewBattle();

            if (_logEvents)
                Debug.Log("[Battle] 다시 시작");
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

                    // 승패는 누가 쓰러진 틱에만 바뀌므로 그때만 가린다.
                    if (DispatchTickEvents() && Outcome == BattleOutcome.Ongoing)
                        JudgeOutcome();
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

        // 전투에 들어갈 캐릭터와 처음 편 · 자리를 한 번 적어 둔다(다시 하기도 이 배치를 쓴다).
        private void CollectCharacters(CharacterManager characterManager)
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
                Vector3 position = character.transform.position;

                _characters.Add(character);
                _teams.Add(character is PlayerCharacter ? BattleTeam.Ally : BattleTeam.Enemy);
                _startPositions.Add(new Vector2(position.x, position.y));
                _presenters.Add(new BattleUnitPresenter(character));
            }
        }

        // 새 시뮬에 적어 둔 배치대로 유닛을 넣는다(유닛 번호 = 칸 번호). 능력치는 지금 인스펙터 값을 읽는다.
        private void StartNewBattle()
        {
            _simulation = new BattleSimulation(_rules, new NearestEnemySelector());
            Outcome = BattleOutcome.Ongoing;

            for (int i = 0; i < _characters.Count; i++)
            {
                // 캐릭터가 파괴됐어도 번호가 어긋나지 않게 기본 능력치로 넣는다(그림은 BattleUnitPresenter 가 건너뛴다).
                var character = _characters[i];
                var statsSettings = character != null ? character.BattleStats : null;
                var stats = (statsSettings ?? new BattleStatsSettings()).ToBattleUnitStats();

                int unitId = _simulation.AddUnit(_teams[i], _startPositions[i], stats);
                Debug.Assert(unitId == i, "[BattleManager] 유닛 번호와 캐릭터 칸 번호가 어긋났습니다.");
            }
        }

        // 한쪽이 모두 쓰러졌으면 판을 끝내고 한 번 알린다.
        private void JudgeOutcome()
        {
            var outcome = BattleOutcomeJudge.Evaluate(_simulation.Units);
            if (outcome == BattleOutcome.Ongoing)
                return;

            Outcome = outcome;

            if (_logEvents)
                Debug.Log($"[Battle] {_simulation.CurrentTick}틱 판 끝: {outcome}");

            OnBattleEnded?.Invoke(outcome);
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

        // 방금 끝난 한 틱의 이벤트를 생긴 순서대로 알린다. 전투 불능(Down) 이벤트가 있었으면 true.
        private bool DispatchTickEvents()
        {
            bool hasDown = false;

            int eventCount = _simulation.EventCount;
            for (int i = 0; i < eventCount; i++)
            {
                var battleEvent = _simulation.GetEvent(i);
                hasDown |= battleEvent.Kind == BattleEventKind.Down;

                if (_logEvents)
                    LogEvent(battleEvent);

                OnBattleEvent?.Invoke(battleEvent);
            }

            return hasDown;
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
