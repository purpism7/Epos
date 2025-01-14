using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Ability;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        void Idle();
        IActController MoveToTarget(Vector3? pos = null, System.Action finishAction = null, int direction = 1, bool isJumpMove = false);
        IActController CastingSkill(Casting.IListener iListener, ICaster iCaster, Skill skill, List<ICombatant> targetList);
        void TakeDamage(ICaster iCaster);
        void Execute();

        bool InAction { get; }
    }
    
    public class ActController : Controller, IActController
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _iActQueue = null;
        private Vector3 _originPos = Vector3.zero;

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
        
        void IController<IActController, IActor>.ChainFixedUpdate()
        {
            if (!IsActivate)
                return;
            
            _currIAct?.ChainFixedUpdate();
        }

        public override void Activate()
        {
            base.Activate();
            
            if (transform.parent)
            {
                _originPos = transform.parent.position;
                _originPos.z = 0;
            }
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="finishAction"></param>
        /// <param name="reverse">Target Pos 에 도착 후, 반대 방향으로 Flip 할지.</param>
        /// <returns></returns>
        IActController IActController.MoveToTarget(Vector3? pos, System.Action finishAction, int direction, bool isJumpMove)
        {
            if (!IsActivate)
                return null;

            var targetPos = _originPos;
            if (pos != null)
                targetPos = pos.Value;
            // else
                // reverse = transform.position.x - targetPos.x > 0;
            
            var data = new Move.Data
            {
                TargetPos = targetPos,
                FinishAction = finishAction,
                DirectionAfterArriving = direction,
                IsJumpMove = isJumpMove,
            };

            AddActAsync<Move, Move.Data>(data).Forget();

            return this;
        }

        IActController IActController.CastingSkill(Casting.IListener iListener, ICaster iCaster, Skill skill, List<ICombatant> targetList)
        {
            if (!IsActivate)
                return null;
            
            var data = new Casting.Data
            {
                IListener = iListener,
                Skill = skill,
                TargetList = targetList,
            };
            
            AddActAsync<Casting, Casting.Data>(data).Forget();

            return this;
        }

        void IActController.TakeDamage(ICaster iCaster)
        {
            if (!IsActivate)
                return;

            var data = new Damage.Data
            {
                ICaster = iCaster,
            };
            
            Execute<Damage, Damage.Data>(data);
        }

        private async UniTask ExecuteAsync()
        {
            if (InAction)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            if (_iActQueue?.Count > 0)
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
            
            act.SetData(data);
            var animationKey = _iActor?.AnimationKey(act);
            data.SetAnimationKey(animationKey); 
            
            if (_iActQueue == null)
            {
                _iActQueue = new();
                _iActQueue.Clear();
            }
            
            _iActQueue?.Enqueue(act);
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
            
            // Debug.Log(name + " = " + iAct?.GetType());
        }
    }
}

