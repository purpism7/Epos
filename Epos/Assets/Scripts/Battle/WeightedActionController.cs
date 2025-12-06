using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using Common;
using Creature;
using Creature.Action;
using Creature.Action.Weight;
using Random = UnityEngine.Random;

namespace Battle
{
    public interface IWeightedActionRequester
    {
        WeightedActionParam GetWeightedActionParam(ICombatant attacker, ETeam eTeam, IWeightedAction iWeightedAction);
        CancellationTokenSource CancellationTokenSource { get; }
    }

    public interface IWeightedActionController
    {
        void Initialize(WeightedActionController.IListener iListener);

        void Execute(ICombatant executer, IWeightedActionRequester iRequester, bool isFirst = false);
    }

    public class WeightedActionController : IWeightedActionController
    {
        public interface IListener
        {
            void End(IActor iActor);
        }

        private IListener _iListener = null;

        private SortedSet<ActionWeight> _iSortedActionWeightSet = new(
            Comparer<ActionWeight>.Create((action, compAction) =>
            {
                return compAction.Weight.CompareTo(action.Weight);
            }));

        void IWeightedActionController.Initialize(IListener iListener)
        {
            _iListener = iListener;

            _iSortedActionWeightSet?.Clear();
            _iSortedActionWeightSet?.Add(new Creature.Action.Weight.ApproachAttack());
            _iSortedActionWeightSet?.Add(new Creature.Action.Weight.CastSkill());
            _iSortedActionWeightSet?.Add(new Creature.Action.Weight.WaitingIdle());
        }

        void IWeightedActionController.Execute(ICombatant executer, IWeightedActionRequester iRequester, bool isFirst)
        {
            ExecuteAsync(executer, iRequester, isFirst).Forget();
        }

        // private async UniTask ExecuteAsync(ICombatant executer, IWeightedActionRequester iRequester, bool isFirst)
        // {
        //     try
        //     {
        //         // if(executer.ETeam == ETeam.Enemy)
        //         //     await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(0, 0.5f)));
                
        //         var cancellationTokenSource = iRequester.CancellationTokenSource;
        //         if (cancellationTokenSource != null)
        //         {
        //             int frame = 30;
        //             if (executer.ETeam == ETeam.Enemy && isFirst)
        //                 frame += UnityEngine.Random.Range(0, 30);
                    
        //             await UniTask.DelayFrame(frame, cancellationToken: cancellationTokenSource.Token);
        //             if (cancellationTokenSource.IsCancellationRequested)
        //             {
        //                 EndAction(executer.IActor);
        //                 return;
        //             }
        //         }
                
        //         var actionWeight = GetHighestPriorityActionWeight();
        //         var iWeightedAction = actionWeight?.Create();
        //         var param = iRequester.GetWeightedActionParam(executer, executer.ETeam, iWeightedAction);

        //         iWeightedAction?.SetParam(param)?
        //             .SetEndAction(EndAction)?
        //             .SetIActor(executer.IActor)?
        //             .Execute();
        //     }
        //     catch(OperationCanceledException)
        //     {
        //         EndAction(executer.IActor);
        //     }
        // }

        private async UniTask ExecuteAsync(ICombatant executer, IWeightedActionRequester iRequester, bool isFirst)
        {
            try
            {
                var cancellationTokenSource = iRequester.CancellationTokenSource;
                if (cancellationTokenSource != null)
                {
                    float seconds = 0.5f;
                    // int frame = 30;
                    if (executer.ETeam == ETeam.Enemy && isFirst)
                        seconds += UnityEngine.Random.Range(0, 0.5f);
                        // frame += UnityEngine.Random.Range(0, 30);
                    
                    // 취소 시 OperationCanceledException 발생 -> catch 블록으로 이동
                    // await UniTask.DelayFrame(frame, cancellationToken: cancellationTokenSource.Token);
                    await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationTokenSource.Token);
                }
                
                var actionWeight = GetHighestPriorityActionWeight();
                // 액션이 없으면 null
                var iWeightedAction = actionWeight?.Create();
                
                // 액션이 유효하다면 실행
                if (iWeightedAction != null)
                {
                    var param = iRequester.GetWeightedActionParam(executer, executer.ETeam, iWeightedAction);
                    iWeightedAction.SetParam(param)
                        .SetEndAction(EndAction)
                        .SetIActor(executer.IActor)
                        .Execute();
                }
                else
                {
                    // 중요: 수행할 액션이 없더라도 턴/행동을 종료 처리는 해야 함
                    // Debug.LogWarning($"[{executer.IActor?.Name}] No valid action weight found. Skipping turn.");
                    EndAction(executer.IActor);
                }
            }
            catch (OperationCanceledException)
            {
                // 취소 발생 시 종료 처리
                EndAction(executer.IActor);
            }
            catch (Exception e)
            {
                // 예상치 못한 에러 발생 시에도 게임이 멈추지 않도록 종료 처리 권장
                Debug.LogError(e);
                EndAction(executer.IActor);
            }
        }
        
        private void EndAction(IActor iActor)
        {
            if (iActor != null &&
               !iActor.IsActivate)
                return;

            _iListener?.End(iActor);
        }

        private ActionWeight GetHighestPriorityActionWeight()
        {
            foreach(var actionWeight in _iSortedActionWeightSet)
            {
                if (actionWeight.CheckCondition())
                    return actionWeight;
            }

            return null;
        }
    }
}
