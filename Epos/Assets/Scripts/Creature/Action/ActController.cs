using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using Cysharp.Threading.Tasks;
using VContainer;

using Datas.ScriptableObjects;
using Creator;

namespace Creature.Action
{
    public interface IActListener
    {
        void OnUpdateAct(IAct act);
    }
    
    public interface IActController : IController<IActController, IActor>
    {
        System.Type CurrentAction { get; }
        IAct GetCurrentAct();
        bool IsAct<T>() where T : IAct;
        string GetCurrentActName();
        
        // Act 이벤트 등록/해제
        void OnActStarted<T>(Action<T> onActStarted) where T : IAct;
        void OnActEnded<T>(Action<T> onActEnded) where T : IAct;
        void RemoveActStarted<T>(Action<T> onActStarted) where T : IAct;
        void RemoveActEnded<T>(Action<T> onActEnded) where T : IAct;
        
        IActController MoveTo(Move.Param param);
        IActController TraceTo(Trace.Param param);
        IActController CastingSkill(Casting.IListener iListener, ICombatant iCombatant, Ability.ISkill iSkill, ICombatant target, List<ICombatant> targetList);

        IActController Die();
        IActController Victory();

        void Impact(Impact.Param impactParam);

        void Execute();
        void ClearActQueue();

        bool InAction { get; }

        void Flip(float x);
        void SetPosition(Vector3 position);

        void SetActListener(IActListener listener);
    }
    
    public class ActController : Controller, IActController
    {
        [Inject] private IObjectResolver _iResolver = null;

        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _actQueue = null;
        private Vector3 _currPosition = Vector3.zero;
        private bool _isCastingCompleted = false;

        // Act 이벤트 딕셔너리
        private Dictionary<System.Type, Action<IAct>> _onActStartedDic = null;
        private Dictionary<System.Type, Action<IAct>> _onActEndedDic = null;

        public bool InAction { get; private set; } = false;

        #region IController
        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iActDic = new();
            _iActDic.Clear();

            Preload();

            return this;
        }

        void IController<IActController, IActor>.ChainUpdate()
        {
            if (!IsActivate)
                return;
            
            _currIAct?.ChainUpdate();
        }
        
        void IController<IActController, IActor>.ChainFixedUpdate()
        {
            if (!IsActivate)
                return;
            
            _currIAct?.ChainFixedUpdate();
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            _actQueue?.Clear();
            _onActStartedDic?.Clear();
            _onActEndedDic?.Clear();
            //Idle();
        }
        #endregion
            
        private void Preload()
        {
            //GetAct<Casting, Casting.Param>();
        }

        #region IActController

        System.Type IActController.CurrentAction
        {
            get
            {
                return _currIAct?.GetType();
            }
        }

        IAct IActController.GetCurrentAct()
        {
            return _currIAct;
        }

        bool IActController.IsAct<T>()
        {
            return _currIAct is T;
        }

        string IActController.GetCurrentActName()
        {
            return _currIAct?.GetType()?.Name ?? string.Empty;
        }

        void IActController.OnActStarted<T>(Action<T> onActStarted)
        {
            if (onActStarted == null)
                return;

            if (_onActStartedDic == null)
            {
                _onActStartedDic = new();
                _onActStartedDic.Clear();
            }

            System.Type type = typeof(T);
            if (_onActStartedDic.TryGetValue(type, out var existingAction))
            {
                _onActStartedDic[type] = existingAction + (act => onActStarted((T)act));
            }
            else
            {
                _onActStartedDic[type] = act => onActStarted((T)act);
            }
        }

        void IActController.OnActEnded<T>(Action<T> onActEnded)
        {
            if (onActEnded == null)
                return;

            if (_onActEndedDic == null)
            {
                _onActEndedDic = new();
                _onActEndedDic.Clear();
            }

            System.Type type = typeof(T);
            if (_onActEndedDic.TryGetValue(type, out var existingAction))
            {
                _onActEndedDic[type] = existingAction + (act => onActEnded((T)act));
            }
            else
            {
                _onActEndedDic[type] = act => onActEnded((T)act);
            }
        }

        void IActController.RemoveActStarted<T>(Action<T> onActStarted)
        {
            if (onActStarted == null || _onActStartedDic == null)
                return;

            System.Type type = typeof(T);
            if (_onActStartedDic.TryGetValue(type, out var existingAction))
            {
                var updated = existingAction - (act => onActStarted((T)act));
                if (updated == null)
                    _onActStartedDic.Remove(type);
                else
                    _onActStartedDic[type] = updated;
            }
        }

        void IActController.RemoveActEnded<T>(Action<T> onActEnded)
        {
            if (onActEnded == null || _onActEndedDic == null)
                return;

            System.Type type = typeof(T);
            if (_onActEndedDic.TryGetValue(type, out var existingAction))
            {
                var updated = existingAction - (act => onActEnded((T)act));
                if (updated == null)
                    _onActEndedDic.Remove(type);
                else
                    _onActEndedDic[type] = updated;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="finishAction"></param>
        /// <param name="reverse">Target Pos 에 도착 후, 반대 방향으로 Flip 할지.</param>
        /// <returns></returns>
        IActController IActController.MoveTo(Move.Param param)
        {
            if (!IsActivate)
                return null;

            if (param == null)
                return null;

            AddActAsync<Move, Move.Param>(param).Forget();

            return this;
        }

        IActController IActController.TraceTo(Trace.Param param)
        {
            if (!IsActivate)
                return null;

            if (param == null)
                return null;

            AddActAsync<Trace, Trace.Param>(param).Forget();

            return this;
        }

        IActController IActController.CastingSkill(Casting.IListener iListener, ICombatant iCaster, Ability.ISkill iSkill, ICombatant target, List<ICombatant> targetList)
        {
            if (!IsActivate)
                return null;

            var castingParam = new Casting.Param
            {
                IListener = iListener,
                ISkill = iSkill,
            }
            .WithAttacker(iCaster)
            .WithTarget(target)
            .WithTargetList(targetList);
            
            AddActAsync<Casting, Casting.Param>(castingParam).Forget();

            return this;
        }

        // IActController IActController.Knockback(Knockback.Param param)
        // {
        //     if (!IsActivate)
        //         return null;
        //
        //     Execute<Knockback, Knockback.Param>(param, false);
        //     
        //     return this;
        // }

        IActController IActController.Die()
        {
            Execute<Die, Die.Param>();
            
            return this;
        }

        IActController IActController.Victory()
        {
            Execute<Victory, Victory.Param>();

            return this;
        }

        void IActController.Impact(Impact.Param impactParam)
        {
            if (!IsActivate)
                return;
            
            Execute<Impact, Impact.Param>(impactParam, false);
        }

        private async UniTask ExecuteAsync()
        {
            //if (InAction)
            //    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            // Casting은 End()로 종료 신호가 올 때까지 다음 Action 전환을 막는다.
            if (_currIAct is Casting && !_isCastingCompleted)
                return;
            
            if (_actQueue?.Count > 0)
            {
                if (_actQueue.TryDequeue(out IAct act))
                {
                    InAction = true;
                    _isCastingCompleted = false;

                    _currIAct?.Deactivate();
                    
                    // 이전 Act 종료 이벤트 발생
                    if (_currIAct != null)
                        NotifyActEnded(_currIAct);

                    //iAct?.SetIsEnd(false);
                    act?.Execute();
                    
                    SetCurrIAct(act);
                    
                    // 새 Act 시작 이벤트 발생
                    if (act != null)
                        NotifyActStarted(act);
                    
                    return;
                }
            }

            Idle();
        }

        public void Execute()
        {
            if (!IsActivate)
                return;
            
            ExecuteAsync().Forget();
        }

        public void ClearActQueue()
        {
            _actQueue?.Clear();
        }

        private void Idle()
        {
            // 이전 Act 종료 이벤트 발생
            if (_currIAct != null)
                NotifyActEnded(_currIAct);
            
            _currIAct?.Deactivate();
            
            Execute<Idle, Idle.Param>();
            SetCurrIAct(null);
            
            _isCastingCompleted = false;
            InAction = false;
        }

        private UniTask AddActAsync<T, V>(V param = null) where T : Act<V>, new() where V : ActParam, new()
        {
            if (_currIAct is Die)
                return UniTask.CompletedTask;
            
            var act = GetAct<T, V>();
            if (act == null)
                return UniTask.CompletedTask;
            
            if (param == null)
                param = new V();
            
            act.SetParam(param);

            var animationKey = _iActor?.AnimationKey(act);
            param.SetAnimationKey(animationKey); 
            
            if (_actQueue == null)
                _actQueue = new();

            _actQueue?.Enqueue(act);

            return UniTask.CompletedTask;
        }

        private Act<V> GetAct<T, V>() where T : Act<V>, new() where V : ActParam, new()
        {
            if (_iActDic == null)
                _iActDic = new();

            System.Type type = typeof(T);
            Act<V> act = null;
            
            if (_iActDic.TryGetValue(type, out IAct iAct))
                act = iAct as Act<V>;
            else
            {
                act = new T();
                _iResolver?.Inject(act);

                act.Initialize(_iActor);
                act.SetEndActAction(EndAct);
                
                _iActDic?.TryAdd(type, act);
            }
            
            return act;
        }

        void IActController.Flip(float x)
        {
            if (!IsActivate)
                return;

            var skeletonAnimation = _iActor?.SkeletonAnimation;
            if (skeletonAnimation == null)
                return;

           
            if (Mathf.Abs(x) > 0.01f)
                skeletonAnimation.Skeleton.ScaleX = Mathf.Sign(x);
        }

        void IActController.SetPosition(Vector3 position)
        {
            _currPosition = position;
            _currPosition.z = 0;
        }

        void IActController.SetActListener(IActListener listener)
        {
            
        }
        #endregion

        private void Execute<T, V>(V param = null, bool isSet = true) where T : Act<V>, new() where V : ActParam, new()
        {
            if (_currIAct is Die)
                return;

            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (param == null)
                param = new V();

            param.SetAnimationKey(_iActor?.AnimationKey(act));
            
            // 이전 Act 종료 이벤트 발생
            if (isSet && _currIAct != null)
                NotifyActEnded(_currIAct);
            
            act.SetParam(param);
            act.Execute();

            if(isSet)
            {
                SetCurrIAct(act);
                // 새 Act 시작 이벤트 발생
                NotifyActStarted(act);
            }
        }
        
        private void EndAct(IActor iActor)
        {
            if (_currIAct is Casting)
                _isCastingCompleted = true;
            
            Execute();
        }

        private void SetCurrIAct(IAct iAct)
        {
            _currIAct = iAct;
            
            // Debug.Log(name + " = " + iAct?.GetType());
        }

        private void NotifyActStarted(IAct act)
        {
            NotifyActEvent(act, _onActStartedDic);
        }

        private void NotifyActEnded(IAct act)
        {
            NotifyActEvent(act, _onActEndedDic);
        }

        private void NotifyActEvent(IAct act, Dictionary<System.Type, Action<IAct>> eventDic)
        {
            if (act == null)
                return;

            System.Type actType = act.GetType();
            
            // 직접 구독한 이벤트 발생
            if (eventDic != null && eventDic.Count > 0)
            {
                // 정확한 타입으로 이벤트 발생
                if (eventDic.TryGetValue(actType, out var action))
                {
                    action?.Invoke(act);
                }
                
                // 부모 타입으로도 이벤트 발생 (IAct 등)
                // IAct 타입만 별도로 체크 (성능 최적화)
                if (actType != typeof(IAct) && eventDic.TryGetValue(typeof(IAct), out var iActAction))
                {
                    iActAction?.Invoke(act);
                }
            }
        }
    }
}

