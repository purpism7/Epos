using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        void MoveToTarget(string key, Vector3 pos, System.Action finishAction = null);
    }
    
    public class ActController : Controller, IActController, Move.IListener
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        
        #region IActController
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
        
        void IActController.MoveToTarget(string key, Vector3 pos, System.Action finishAction)
        {
            Execute<Move, Move.Data>(
                new Move.Data()
                {
                    Key = key,
                    IListener = this,
                    TargetPos = pos,
                    FinishAction = finishAction,
                });
        }
        #endregion

        private void Execute<T, V>(V data = null) where T : Act<V>, new() where V : ActData, new()
        {
            if (_iActDic == null)
            {
                _iActDic = new();
                _iActDic.Clear();
            }

            if (data != null)
            {
                data.IActor = _iActor;
            }
            else
            {
                data = new V()
                {
                    IActor = _iActor,
                };
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
            
            _currIAct?.Finish();
            act?.Execute(data);
            _currIAct = act;
        }

        #region Move.IListener
        void Move.IListener.Arrived()
        {
            Execute<Idle, Idle.Data>( 
                new Idle.Data()
                {
                    Key = _iActor is Hero ? "00_F_Idle" : "00_Idle",
                });
        }
        #endregion
    }
}

