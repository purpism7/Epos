using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        void MoveToTarget(Vector3 pos, System.Action finishAction = null);
        void Idle();
    }
    
    public class ActController : Controller, IActController, Move.IListener
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        
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
            _currIAct?.ChainUpdate();
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Activate();
        }
        #endregion
            
        #region IActController
        void IActController.MoveToTarget(Vector3 pos, System.Action finishAction)
        {
            if (!IsActivate)
                return;
            
            Execute<Move, Move.Data>(
                new Move.Data()
                {
                    IListener = this,
                    TargetPos = pos,
                    FinishAction = finishAction,
                });
        }

        void IActController.Idle()
        {
            Execute<Idle, Idle.Data>();
        }
        #endregion

        private void Execute<T, V>(V data = null) where T : Act<V>, new() where V : Act<V>.BaseData, new()
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
                
                _iActDic?.TryAdd(type, act);
            }

            var animationKey = _iActor.AnimationKey(act);
            if (data != null)
            {
                data.IActor = _iActor;
                data.Key = animationKey;
            }
            else
            {
                data = new V()
                {
                    IActor = _iActor,
                    Key = animationKey,
                };
            }
            
            _currIAct?.Finish();
            act?.Execute(data);
            _currIAct = act;
        }

        #region Move.IListener
        void Move.IListener.Arrived()
        {
            Execute<Idle, Idle.Data>();
        }
        #endregion
    }
}

