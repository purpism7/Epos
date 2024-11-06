using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ability;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        void Idle();
        IActController MoveToTarget(Vector3 pos, System.Action finishAction = null, bool immediately = false);
        IActController CastingSkill(Casting.IListener iListener, Skill skill, List<ICombatant> targetList);
        void Execute();

        bool InAction { get; }
    }
    
    public class ActController : Controller, IActController
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _iActQueue = null;

        public bool InAction { get; private set; } = false;

        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iActDic = new();
            _iActDic.Clear();
            
            return this;
        }

        #region IController

        void IController<IActController, IActor>.ChainUpdate()
        {
            if (!IsActivate)
                return;
            
            _currIAct?.ChainUpdate();
        }

        public override void Activate()
        {
            base.Activate();
            
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            _iActQueue?.Clear();
            Idle();
        }
        #endregion
            
        #region IActController
        void IActController.Idle()
        {
            Idle();
        }
        
        IActController IActController.MoveToTarget(Vector3 pos, System.Action finishAction, bool immediately)
        {
            if (!IsActivate)
                return this;

            var data = new Move.Data
            {
                TargetPos = pos,
                FinishAction = finishAction,
            };

            if (immediately)
            {
                Execute<Move, Move.Data>(data);
            }
            else
            {
                AddActAsync<Move, Move.Data>(data).Forget();
            }

            return this;
        }

        IActController IActController.CastingSkill(Casting.IListener iListener, Skill skill, List<ICombatant> targetList)
        {
            if (!IsActivate)
                return this;
            
            var data = new Casting.Data
            {
                IListener = iListener,
                Skill = skill,
                TargetList = targetList,
            };
            
            AddActAsync<Casting, Casting.Data>(data).Forget();

            return this;
        }

        private async UniTask ExecuteAsync()
        {
            if (InAction)
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            
            if (_iActQueue.Count > 0)
            {
                if (_iActQueue.TryDequeue(out IAct iAct))
                {
                    InAction = true;
                    
                    iAct?.Execute();
                    SetCurrIAct(iAct);

                    return;
                }
            }

            Idle();
        }

        public void Execute()
        {
            ExecuteAsync().Forget();
        }

        private void Idle()
        {
            Execute<Idle, Idle.Data>();
            SetCurrIAct(null);
            
            InAction = false;
        }

        private async UniTask AddActAsync<T, V>(V data = null) where T : Act<V>, new() where V : Act<V>.BaseData, new()
        {
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (data == null)
            {
                data = new V();
            }

            data.SetAnimationKey(_iActor?.AnimationKey(act));
            
            act.SetData(data);
            
            if (_iActQueue == null)
            {
                _iActQueue = new();
                _iActQueue.Clear();
            }
            
            _iActQueue?.Enqueue(act);

            // await UniTask.Yield();
            //
            // if (_currIAct == null)
            // {
            //     ExecuteAct();
            // }
        }

        private Act<V> GetAct<T, V>() where T : Act<V>, new() where V : Act<V>.BaseData, new()
        {
            if (_iActDic == null)
            {
                _iActDic = new();
                _iActDic.Clear();
            }
            
            System.Type type = typeof(T);
            Act<V> act = null;
            
            if (_iActDic.TryGetValue(type, out IAct iAct))
            {
                act = iAct as Act<V>;
            }
            else
            {
                act = new T();
                act.Initialize(_iActor);
                act.SetEndActAction(EndAct);
                
                _iActDic?.TryAdd(type, act);
            }
            
            return act;
        }
        #endregion

        private void Execute<T, V>(V data = null) where T : Act<V>, new() where V : Act<V>.BaseData, new()
        {
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (data == null)
            {
                data = new V();
            }

            data.SetAnimationKey(_iActor?.AnimationKey(act));
            
            act.SetData(data);
            act.Execute();
            
            SetCurrIAct(act);
        }
        
        private void EndAct()
        {
            Execute();
        }

        private void SetCurrIAct(IAct iAct)
        {
            _currIAct = iAct;
        }

        // #region Move.IListener
        // void Move.IListener.Arrived()
        // {
        //     // Execute<Idle, Idle.Data>();
        // }
        // #endregion
    }
}

