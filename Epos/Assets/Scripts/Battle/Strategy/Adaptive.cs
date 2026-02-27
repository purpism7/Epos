using Common;
using Creature;
using Creature.Action;
using Cysharp.Threading.Tasks;
using GameSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Strategy
{
    public class Adaptive : BaseStrategy
    {
        public override void Apply(IStrategyDataProvider iStrategyDataProvider)
        {
            base.Apply(iStrategyDataProvider);
    
            LeaderICombatant = iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10001);
        }

        public override void InitializeFormationPosition()
        {
            base.InitializeFormationPosition();
            
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            SetFormationPosition(iCombatant, DirectionType.Back, 6f);
            
            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            SetFormationPosition(iCombatant, DirectionType.Left, 6f);
        }

        public override void MoveFormation(Vector3 targetPosition)
        {
            base.MoveFormation(targetPosition);
            
            var iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            TraceTo(iCombatant, DirectionType.Back, 4f, false);
            
            iCombatant = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            TraceTo(iCombatant, DirectionType.Left, 4f, false);
        }

        public override async UniTask RegroupToLeaderAsync()
        {
            await base.RegroupToLeaderAsync();

            // 1. 몇 명이 이동해야 하고, 몇 명이 도착했는지 체크할 변수
            int totalMoveCount = 0;
            int completedCount = 0;

            // 2. 액션이 끝날 때마다 완료 카운트를 올려줄 콜백 함수 생성
            Action<Creature.Action.Trace> onTraceEnded = (act) =>
            {
                completedCount++;
            };

            // --- 첫 번째 유닛 이동 지시 ---
            var combatant1 = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10004);
            if (combatant1?.IActor?.IActCtr != null)
            {
                TraceTo(combatant1, DirectionType.Back, 4f, true);
                //combatant1?.IActor?.IActCtr?.TraceTo(traceParam);               // 1. 이동 명령
                combatant1?.IActor?.IActCtr?.OnActEnded(onTraceEnded);           // 2. 종료 이벤트 구독
                totalMoveCount++;                                      // 3. 목표 카운트 증가
            }

            // --- 두 번째 유닛 이동 지시 ---
            var combatant2 = _iStrategyDataProvider?.AllyICombatantList?.Find(combatant => combatant.IActor.Id == 10003);
            if (combatant2?.IActor?.IActCtr != null)
            {
                TraceTo(combatant2, DirectionType.Left, 4f, true);
                //combatant2?.IActor?.IActCtr?.TraceTo(traceParam);
                combatant2?.IActor?.IActCtr?.OnActEnded(onTraceEnded);
                totalMoveCount++;
            }

            // 🌟 3. 목표한 유닛들이 모두 도착할 때까지 매 프레임 대기합니다.
            if (totalMoveCount > 0)
                await UniTask.WaitUntil(() => completedCount >= totalMoveCount);

            // 🚨 4. 메모리 누수 방지: 대기가 끝났으면 반드시 이벤트를 해제해 줍니다!
            if (combatant1 != null)
                combatant1?.IActor?.IActCtr?.RemoveActEnded(onTraceEnded);

            if (combatant2 != null)
                combatant2.IActor?.IActCtr?.RemoveActEnded(onTraceEnded);

            UnityEngine.Debug.Log("모두 집결 완료!");
        }
    }
}