using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Datas.ScriptableObjects;

namespace Creature.Action
{
    public interface IActController : IController<IActController, IActor>
    {
        IActController MoveToTargetPosition(Move.Param param);
        IActController MoveToTarget(Move.Param param);
        IActController CastingSkill(Casting.IListener iListener, ICombatant iCombatant, Skill skill, List<ICombatant> targetList);
        IActController Die();
        
        void TakeDamage(ICaster iCaster, bool playAnimation);
        void Execute();

        bool InAction { get; }

        void SetPosition(Vector3 position);
    }
    
    public class ActController : Controller, IActController
    {
        private IActor _iActor = null;
        private Dictionary<System.Type, IAct> _iActDic = null;
        private IAct _currIAct = null;
        private Queue<IAct> _iActQueue = null;
        private Vector3 _currPosition = Vector3.zero;

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
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            _iActQueue?.Clear();
            Idle();
        }
        #endregion
            
        #region IActController
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="finishAction"></param>
        /// <param name="reverse">Target Pos 에 도착 후, 반대 방향으로 Flip 할지.</param>
        /// <returns></returns>
        IActController IActController.MoveToTargetPosition(Move.Param moveParam)
        {
            if (!IsActivate)
                return null;

            if (moveParam == null)
                return null;

            // var targetPos = _currPosition;
            // if (moveParam.TargetPos != null)
            //     targetPos = moveParam.TargetPos.Value;
         
            AddActAsync<Move, Move.Param>(moveParam).Forget();

            return this;
        }

        IActController IActController.MoveToTarget(Move.Param moveParam)
        {
            if (!IsActivate)
                return null;

            if (moveParam == null)
                return null;

            AddActAsync<Move, Move.Param>(moveParam).Forget();

            return this;
        }

        IActController IActController.CastingSkill(Casting.IListener iListener, ICombatant iCombatant, Skill skill, List<ICombatant> targetList)
        {
            if (!IsActivate)
                return null;
            
            var castingParam = new Casting.Param
            {
                IListener = iListener,
                ICombatant = iCombatant,
                Skill = skill,
                TargetList = targetList,
            };
            
            AddActAsync<Casting, Casting.Param>(castingParam).Forget();

            return this;
        }

        IActController IActController.Die()
        {
            Execute<Die, Die.Param>();
            
            return this;
        }

        void IActController.TakeDamage(ICaster iCaster, bool PlayAnimation)
        {
            if (!IsActivate)
                return;

            var damageParam = new Damage.Param
            {
                ICaster = iCaster,
                PlayAnimation = PlayAnimation,
            };
            
            Execute<Damage, Damage.Param>(damageParam);
        }

        private async UniTask ExecuteAsync()
        {
            if (InAction)
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            
            if (_iActQueue?.Count > 0)
            {
                if (_iActQueue.TryDequeue(out IAct iAct))
                {
                    InAction = true;
                    
                    iAct?.Execute();
                    _currIAct?.Deactivate();
                    
                    SetCurrIAct(iAct);
                    
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

        private void Idle()
        {
            _currIAct?.Deactivate();
            
            Execute<Idle, Idle.Param>();
            SetCurrIAct(null);
            
            InAction = false;
        }

        private async UniTask AddActAsync<T, V>(V param = null) where T : Act<V>, new() where V : ActParam, new()
        {
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (param == null)
                param = new V();
            
            act.SetParam(param);
            var animationKey = _iActor?.AnimationKey(act);
            param.SetAnimationKey(animationKey); 
            
            if (_iActQueue == null)
            {
                _iActQueue = new();
                _iActQueue.Clear();
            }
            
            _iActQueue?.Enqueue(act);
        }

        private Act<V> GetAct<T, V>() where T : Act<V>, new() where V : ActParam, new()
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
        
        void IActController.SetPosition(Vector3 position)
        {
            _currPosition = position;
            _currPosition.z = 0;
        }
        #endregion

        private void Execute<T, V>(V param = null) where T : Act<V>, new() where V : ActParam, new()
        {
            // if (!IsActivate)
            //     return;
            
            var act = GetAct<T, V>();
            if (act == null)
                return;
            
            if (param == null)
                param = new V();

            param.SetAnimationKey(_iActor?.AnimationKey(act));
            
            act.SetParam(param);
            act.Execute();
            
            SetCurrIAct(act);
        }
        
        private void EndAct(IActor iActor)
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

