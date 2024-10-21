using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        void MoveToTarget(Vector3 pos, System.Action finishAction = null, bool immediately = false);
        void Idle();
        void UseSkill(Skill.IListener iListener);
    }
    
    public class ActController : Controller, IActController
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _iActQueue = null;
        
        #region IController
        IActController IController<IActController, IActor>.Initialize(IActor iActor)
        {
            _iActor = iActor;

            _iActDic = new();
            _iActDic.Clear();
            
            return this;
        }

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
        void IActController.MoveToTarget(Vector3 pos, System.Action finishAction, bool immediately)
        {
            if (!IsActivate)
                return;

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
                AddAct<Move, Move.Data>(data);
            }
        }

        void IActController.Idle()
        {
            Idle();
        }

        void IActController.UseSkill(Skill.IListener iListener)
        {
            var data = new Skill.Data
            {
                IListener = iListener,
            };
            
            AddAct<Skill, Skill.Data>(data);
        }
        
        private void Idle()
        {
            Execute<Idle, Idle.Data>();
            SetCurrIAct(null);
        }

        private void AddAct<T, V>(V data = null) where T : Act<V>, new() where V : Act<V>.BaseData, new()
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

            if (_currIAct == null)
            {
                ExecuteActAsync().Forget();
            }
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

        private async UniTask ExecuteActAsync()
        {
            await UniTask.Yield();
            
            if (_iActQueue.Count > 0)
            {
                if (_iActQueue.TryDequeue(out IAct iAct))
                {
                    iAct?.Execute();
                    SetCurrIAct(iAct);

                    return;
                }
            }

            Idle();
        }
        
        private void EndAct()
        {
            ExecuteActAsync().Forget();
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

